using System.Globalization;
using FlyzenApi.Application.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMealOptionRepository _mealOptionRepository;
        private readonly IBaggageOptionRepository _baggageOptionRepository;
        private readonly IPromoCodeRepository _promoCodeRepository;
        private readonly ISkyPointsRepository _skyPointsRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly INotificationService _notificationService;
        private readonly ITranslationService _translationService;

        public BookingService(
            IBookingRepository bookingRepository,
            IFlightRepository flightRepository,
            IUserRepository userRepository,
            IMealOptionRepository mealOptionRepository,
            IBaggageOptionRepository baggageOptionRepository,
            IPromoCodeRepository promoCodeRepository,
            ISkyPointsRepository skyPointsRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            INotificationService notificationService,
            ITranslationService translationService)
        {
            _bookingRepository = bookingRepository;
            _flightRepository = flightRepository;
            _userRepository = userRepository;
            _mealOptionRepository = mealOptionRepository;
            _baggageOptionRepository = baggageOptionRepository;
            _promoCodeRepository = promoCodeRepository;
            _skyPointsRepository = skyPointsRepository;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _notificationService = notificationService;
            _translationService = translationService;
        }

        public async Task<BookingDto> CreateAsync(Guid userId, CreateBookingRequest request)
        {
            var bookingId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var flight = await _flightRepository.GetByIdAsync(request.FlightId)
                    ?? throw new NotFoundException("Flight not found.");

                // Re-validate the same cutoff enforced at search time (FlightRepository.SearchAsync) -
                // a flight loaded before crossing the cutoff must not become bookable just because
                // the client already had it open.
                if (flight.DepartureTime <= DateTime.UtcNow.AddHours(Flight.BookingCutoffHours))
                    throw new BadRequestException("Bu uçuş üçün bron etmə vaxtı keçib.");

                // Same re-validation reasoning as the cutoff check above - a flight loaded
                // before an admin cancelled it must not become bookable just because the
                // client already had it open (SearchAsync excludes Cancelled flights, but
                // that's a stale-client-side-cache guard, not the source of truth).
                if (flight.OperationalStatus == FlightOperationalStatus.Cancelled)
                    throw new BadRequestException("Bu uçuş ləğv edilib.");

                var user = await _userRepository.GetByIdAsync(userId)
                    ?? throw new NotFoundException("User not found.");

                var seatIds = request.Passengers.Select(p => p.SeatId).ToList();
                if (seatIds.Distinct().Count() != seatIds.Count)
                    throw new BadRequestException("Each passenger must have a distinct seat.");

                var seatsById = flight.Seats.ToDictionary(s => s.Id);
                var mealOptions = (await _mealOptionRepository.GetAllAsync()).ToDictionary(m => m.Id);
                var baggageOptions = (await _baggageOptionRepository.GetAllAsync()).ToDictionary(b => b.Id);

                decimal total = 0m;
                var passengers = new List<BookingPassenger>();

                foreach (var p in request.Passengers)
                {
                    if (!seatsById.TryGetValue(p.SeatId, out var seat))
                        throw new BadRequestException("One of the selected seats does not belong to this flight.");
                    if (!seat.IsAvailable)
                        throw new ConflictException($"Seat {seat.SeatNumber} is no longer available.");

                    MealOption? meal = null;
                    if (p.MealOptionId.HasValue && !mealOptions.TryGetValue(p.MealOptionId.Value, out meal))
                        throw new BadRequestException("Invalid meal option selected.");

                    BaggageOption? baggage = null;
                    if (p.BaggageOptionId.HasValue && !baggageOptions.TryGetValue(p.BaggageOptionId.Value, out baggage))
                        throw new BadRequestException("Invalid baggage option selected.");

                    var seatPrice = Math.Round(flight.BasePrice * seat.PriceMultiplier, 2);
                    var passengerPrice = seatPrice + (meal?.Price ?? 0m) + (baggage?.Price ?? 0m);
                    total += passengerPrice;

                    seat.IsAvailable = false;

                    passengers.Add(new BookingPassenger
                    {
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        PassportNumber = p.PassportNumber,
                        Type = p.Type,
                        SeatMapId = seat.Id,
                        MealOptionId = meal?.Id,
                        BaggageOptionId = baggage?.Id,
                        PriceCalculated = passengerPrice,
                    });
                }

                // The frontend may show a discounted preview from /api/promo-codes/validate,
                // but that's informational only - the code is re-validated and the discount
                // recomputed here, from scratch, before anything is actually charged.
                if (!string.IsNullOrWhiteSpace(request.PromoCode))
                {
                    var promoCode = await _promoCodeRepository.GetByCodeAsync(request.PromoCode)
                        ?? throw new BadRequestException("Invalid or expired promo code.");
                    if (!promoCode.IsCurrentlyValid())
                        throw new BadRequestException("Invalid or expired promo code.");

                    total -= Math.Round(total * promoCode.DiscountPercentage / 100m, 2);
                    promoCode.UsedCount++;
                }

                // Stacks with the promo code above (applied to the post-promo
                // remainder, not the original total - order matters: promo is a
                // percentage, this is a fixed AZN amount). Like the promo code,
                // the frontend's live slider preview is informational only -
                // clamped from scratch here against both the user's real balance
                // and the remaining total, so neither can go negative or below 0
                // regardless of what the client requested.
                var pointsToRedeem = 0;
                if (request.SkyPointsToRedeem is > 0)
                {
                    var maxByBalance = user.SkyPointsBalance;
                    var maxByTotal = (int)Math.Floor(total * 100m);
                    pointsToRedeem = Math.Min(request.SkyPointsToRedeem.Value, Math.Min(maxByBalance, maxByTotal));
                    if (pointsToRedeem > 0)
                        total -= pointsToRedeem / 100m;
                }

                var bookingId = Guid.NewGuid();
                foreach (var seatId in seatIds)
                    seatsById[seatId].BookingId = bookingId;

                var booking = new Booking
                {
                    // Assigned client-side (rather than left for the DB's gen_random_uuid()
                    // default) so the ticket's QrCodeData below can embed the real booking
                    // id instead of the pre-insert default Guid.Empty.
                    Id = bookingId,
                    UserId = userId,
                    FlightId = flight.Id,
                    PNR = GeneratePnr(),
                    TotalPrice = total,
                    // Starts Pending so admin staff can review/confirm, matching the existing
                    // admin-dashboard approval workflow (Təsdiqlə/Ləğv Et).
                    Status = BookingStatus.Pending,
                    Passengers = passengers,
                };

                // Attached via the nav property so the booking, its passengers, seat
                // updates, and the ticket all persist in a single SaveChanges call.
                booking.Ticket = new Ticket
                {
                    TicketNumber = GenerateTicketNumber(),
                    QrCodeData = $"SKYBOOK|{booking.PNR}|{booking.Id}",
                    IssuedAt = DateTime.UtcNow,
                };

                await _bookingRepository.AddAsync(booking);

                // Earned on the amount actually paid for this booking - `total` at
                // this point already reflects the promo code discount and any Sky
                // Points redemption above, so this is the real value received, not
                // the pre-discount price. Awarded at creation since this app has no
                // payment gateway - booking creation already stands in for "paid"
                // everywhere else (see the notification comment below). Must come
                // after AddAsync: RelatedBookingId is a real FK to Bookings, so the
                // booking row has to exist first.
                var skyPointsEarned = (int)Math.Round(total * 10m, MidpointRounding.AwayFromZero);
                if (skyPointsEarned > 0)
                {
                    user.SkyPointsBalance += skyPointsEarned;
                    await _skyPointsRepository.AddTransactionAsync(new SkyPointsTransaction
                    {
                        UserId = userId,
                        Amount = skyPointsEarned,
                        Type = SkyPointsTransactionType.Earned,
                        RelatedBookingId = booking.Id,
                    });
                }

                if (pointsToRedeem > 0)
                {
                    user.SkyPointsBalance -= pointsToRedeem;
                    await _skyPointsRepository.AddTransactionAsync(new SkyPointsTransaction
                    {
                        UserId = userId,
                        Amount = pointsToRedeem,
                        Type = SkyPointsTransactionType.Redeemed,
                        RelatedBookingId = booking.Id,
                    });
                }

                return booking.Id;
            });

            var created = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking not found after creation.");

            // Fires at checkout (booking creation), not at the admin's later
            // Pending -> Confirmed status flip - this app has no real payment
            // gateway, so submitting the booking *is* the user's "I just paid" moment.
            var seatNumbers = string.Join(", ", created.Passengers.Select(p => p.Seat?.SeatNumber).Where(n => n is not null));
            var route = $"{created.Flight.DepartureCity.Name} -> {created.Flight.ArrivalCity.Name}";
            var language = created.User.LanguagePreference;
            await _notificationService.CreateAsync(
                created.User,
                "notifications.reservationConfirmed.title",
                "notifications.reservationConfirmed.body",
                new Dictionary<string, string> { ["pnr"] = created.PNR, ["route"] = route },
                NotificationType.ReservationConfirmed,
                bookingId: created.Id,
                emailSubject: _translationService.Translate(language, "email.bookingConfirmed.subject", new Dictionary<string, string> { ["pnr"] = created.PNR }),
                emailHtmlBody: EmailTemplates.BuildBookingConfirmationEmail(
                    _translationService,
                    language,
                    created.User.FirstName,
                    created.PNR,
                    route,
                    created.Flight.DepartureTime.ToString("f"),
                    seatNumbers,
                    $"{created.TotalPrice.ToString("0.00", CultureInfo.InvariantCulture)} {created.Currency}"));

            return created.ToDto();
        }

        public async Task<IEnumerable<BookingDto>> GetMineAsync(Guid userId) =>
            (await _bookingRepository.GetByUserIdAsync(userId)).Select(b => b.ToDto());

        public async Task<BookingDto?> GetByIdAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                return null;
            return booking.ToDto();
        }

        public async Task<CheckInStatusDto> GetCheckInStatusAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                throw new NotFoundException("Booking not found.");

            return BuildCheckInStatus(booking);
        }

        public async Task<CheckInStatusDto> CheckInAsync(Guid id, Guid userId)
        {
            var bookingId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var booking = await _bookingRepository.GetByIdAsync(id);
                if (booking is null || booking.UserId != userId)
                    throw new NotFoundException("Booking not found.");

                var status = BuildCheckInStatus(booking);
                switch (status.Availability)
                {
                    case CheckInAvailability.AlreadyCheckedIn:
                        throw new BadRequestException("You have already checked in for this booking.");
                    case CheckInAvailability.NotEligible:
                        throw new BadRequestException(booking.Status == BookingStatus.Cancelled
                            ? "This booking cannot be checked in (it has been cancelled)."
                            : "This booking cannot be checked in until it is confirmed.");
                    case CheckInAvailability.NotYetOpen:
                        throw new BadRequestException("Check-in is not open yet for this flight.");
                    case CheckInAvailability.Closed:
                        throw new BadRequestException("Check-in has closed for this flight.");
                }

                booking.IsCheckedIn = true;
                booking.CheckedInAt = DateTime.UtcNow;
                booking.BoardingPassCode = GenerateBoardingPassCode();
                await _bookingRepository.UpdateAsync(booking);
                return booking.Id;
            });

            var updated = await _bookingRepository.GetByIdAsync(bookingId)
                ?? throw new NotFoundException("Booking not found after check-in.");

            var route = $"{updated.Flight.DepartureCity.Name} -> {updated.Flight.ArrivalCity.Name}";
            await _notificationService.CreateAsync(
                updated.User,
                "notifications.checkInCompleted.title",
                "notifications.checkInCompleted.body",
                new Dictionary<string, string> { ["route"] = route },
                NotificationType.CheckInCompleted,
                bookingId: updated.Id);

            return BuildCheckInStatus(updated);
        }

        public async Task<List<BoardingPassDto>> GetBoardingPassAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                throw new NotFoundException("Booking not found.");

            if (!booking.IsCheckedIn || booking.BoardingPassCode is null)
                throw new BadRequestException("This booking has not been checked in yet.");

            var flight = booking.Flight;
            return booking.Passengers.Select(p => new BoardingPassDto
            {
                BookingId = booking.Id,
                PNR = booking.PNR,
                PassengerName = $"{p.FirstName} {p.LastName}",
                FlightNumber = flight.FlightNumber,
                DepartureCityName = flight.DepartureCity.Name,
                DepartureAirportCode = flight.DepartureCity.AirportCode,
                ArrivalCityName = flight.ArrivalCity.Name,
                ArrivalAirportCode = flight.ArrivalCity.AirportCode,
                DepartureTimeUtc = flight.DepartureTime,
                Gate = flight.GateNumber,
                SeatNumber = p.Seat?.SeatNumber ?? "-",
                SeatClass = p.Seat?.Class.ToString() ?? "-",
                BoardingPassCode = booking.BoardingPassCode,
                // Deliberately compact and non-sensitive - just enough to look
                // up and verify the booking, no passenger/payment data encoded.
                QrPayload = $"{booking.Id}|{booking.BoardingPassCode}",
            }).ToList();
        }

        // Priority order matters: AlreadyCheckedIn always wins (even if the
        // flight/booking state changed afterward - once checked in, that fact
        // doesn't un-happen), then NotEligible (Pending/Cancelled bookings were
        // never checkable, regardless of timing), then the time window itself.
        private static CheckInStatusDto BuildCheckInStatus(Booking booking)
        {
            var flight = booking.Flight;
            var opensAtUtc = flight.DepartureTime.AddHours(-Flight.CheckInOpensHoursBeforeDeparture);
            var closesAtUtc = flight.DepartureTime.AddHours(-Flight.BookingCutoffHours);
            var nowUtc = DateTime.UtcNow;

            CheckInAvailability availability;
            if (booking.IsCheckedIn)
                availability = CheckInAvailability.AlreadyCheckedIn;
            else if (booking.Status != BookingStatus.Confirmed)
                // Pending (not yet admin-confirmed) and Cancelled are both
                // ineligible - only a Confirmed booking can check in.
                availability = CheckInAvailability.NotEligible;
            else if (nowUtc < opensAtUtc)
                availability = CheckInAvailability.NotYetOpen;
            else if (nowUtc >= closesAtUtc || flight.Status != FlightStatus.Scheduled)
                availability = CheckInAvailability.Closed;
            else
                availability = CheckInAvailability.Open;

            return new CheckInStatusDto
            {
                Availability = availability,
                OpensAtUtc = opensAtUtc,
                ClosesAtUtc = closesAtUtc,
                CheckedInAt = booking.CheckedInAt,
            };
        }

        private static string GenerateBoardingPassCode()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = Random.Shared;
            return new string(Enumerable.Range(0, 10).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        public async Task<BookingDto> CancelAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                throw new NotFoundException("Booking not found.");

            if (booking.Status == BookingStatus.Cancelled)
                throw new BadRequestException("Booking is already cancelled.");

            booking.Status = BookingStatus.Cancelled;
            foreach (var passenger in booking.Passengers)
            {
                if (passenger.Seat is not null)
                {
                    passenger.Seat.IsAvailable = true;
                    passenger.Seat.BookingId = null;
                }
            }

            await _bookingRepository.UpdateAsync(booking);

            // Claw back any Sky Points this booking earned - otherwise book-then-
            // cancel would be a free way to farm points. Clamped at 0 rather than
            // assumed-reversible: the user may have already redeemed some of that
            // balance on a different booking in the meantime, so the Reversed
            // transaction logs the amount actually removed, not the original
            // Earned amount, to keep the history honest about what really happened.
            var earnedAmount = await _skyPointsRepository.GetEarnedAmountForBookingAsync(booking.Id);
            if (earnedAmount > 0)
            {
                var amountToReverse = Math.Min(earnedAmount, booking.User.SkyPointsBalance);
                if (amountToReverse > 0)
                {
                    booking.User.SkyPointsBalance -= amountToReverse;
                    await _skyPointsRepository.AddTransactionAsync(new SkyPointsTransaction
                    {
                        UserId = booking.UserId,
                        Amount = amountToReverse,
                        Type = SkyPointsTransactionType.Reversed,
                        RelatedBookingId = booking.Id,
                    });
                }
            }

            return booking.ToDto();
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                throw new NotFoundException("Booking not found.");

            if (booking.Status != BookingStatus.Cancelled)
                throw new BadRequestException("Only cancelled bookings can be deleted.");

            // Soft-delete: keeps the row for admin reporting (GET /api/admin/bookings)
            // while removing it from the user's own GetMineAsync results.
            booking.IsDeleted = true;
            await _bookingRepository.UpdateAsync(booking);
        }

        public async Task SendTicketEmailAsync(Guid id, Guid userId, string email)
        {
            var booking = await _bookingRepository.GetByIdAsync(id);
            if (booking is null || booking.UserId != userId)
                throw new NotFoundException("Booking not found.");
            if (booking.Ticket is null)
                throw new NotFoundException("This booking does not have a ticket yet.");

            var dto = booking.ToDto();
            var subject = $"Your SkyBook ticket - {dto.PNR}";
            var body = $@"
                <div style=""font-family:sans-serif"">
                  <h2 style=""color:#ff6b00"">SkyBook Ticket</h2>
                  <p><b>PNR:</b> {dto.PNR}<br/>
                     <b>Ticket Number:</b> {dto.Ticket!.TicketNumber}</p>
                  <p><b>Flight:</b> {dto.Flight.FlightNumber} ({dto.Flight.DepartureCity.Name} → {dto.Flight.ArrivalCity.Name})<br/>
                     <b>Departure:</b> {dto.Flight.DepartureTime:f}</p>
                  <p><b>Passengers:</b><br/>
                     {string.Join("<br/>", dto.Passengers.Select(p => $"{p.FirstName} {p.LastName} - Seat {p.SeatNumber}"))}</p>
                  <p><b>Total Paid:</b> {dto.TotalPrice:C}</p>
                </div>";

            await _emailService.SendAsync(email, subject, body);
        }

        private static string GeneratePnr()
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = Random.Shared;
            return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
        }

        private static string GenerateTicketNumber() =>
            $"SKB{DateTime.UtcNow:yyyyMMdd}{Random.Shared.Next(100000, 999999)}";
    }
}
