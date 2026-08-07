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
    public class AdminService : IAdminService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFlightRepository _flightRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IAirlineRepository _airlineRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        private readonly INotificationService _notificationService;
        private readonly ITranslationService _translationService;
        private readonly ICurrencyConversionService _currencyConversionService;
        private readonly ISkyPointsRepository _skyPointsRepository;

        public AdminService(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IFlightRepository flightRepository,
            ICityRepository cityRepository,
            IAirlineRepository airlineRepository,
            IUnitOfWork unitOfWork,
            IAuthService authService,
            INotificationService notificationService,
            ITranslationService translationService,
            ICurrencyConversionService currencyConversionService,
            ISkyPointsRepository skyPointsRepository)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _flightRepository = flightRepository;
            _cityRepository = cityRepository;
            _airlineRepository = airlineRepository;
            _unitOfWork = unitOfWork;
            _authService = authService;
            _notificationService = notificationService;
            _translationService = translationService;
            _currencyConversionService = currencyConversionService;
            _skyPointsRepository = skyPointsRepository;
        }

        public async Task<IEnumerable<AdminBookingDto>> GetAllBookingsAsync() =>
            (await _bookingRepository.GetAllAsync()).Select(b => b.ToAdminDto());

        public async Task<AdminBookingDto> UpdateBookingStatusAsync(Guid bookingId, BookingStatus status)
        {
            var updatedId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var booking = await _bookingRepository.GetByIdAsync(bookingId)
                    ?? throw new NotFoundException("Booking not found.");

                if (status == BookingStatus.Cancelled && booking.Status != BookingStatus.Cancelled)
                {
                    foreach (var passenger in booking.Passengers)
                    {
                        if (passenger.Seat is not null)
                        {
                            passenger.Seat.IsAvailable = true;
                            passenger.Seat.BookingId = null;
                        }
                    }
                }
                else if (status != BookingStatus.Cancelled && booking.Status == BookingStatus.Cancelled)
                {
                    // Re-activating a cancelled booking: the freed seats may have been sold
                    // to someone else in the meantime, so they must be re-checked and
                    // re-reserved rather than assumed still held by this booking.
                    foreach (var passenger in booking.Passengers)
                    {
                        if (passenger.Seat is not null && !passenger.Seat.IsAvailable)
                            throw new ConflictException($"Seat {passenger.Seat.SeatNumber} has since been booked by someone else and can no longer be restored to this booking.");
                    }

                    foreach (var passenger in booking.Passengers)
                    {
                        if (passenger.Seat is not null)
                        {
                            passenger.Seat.IsAvailable = false;
                            passenger.Seat.BookingId = booking.Id;
                        }
                    }
                }

                booking.Status = status;
                await _bookingRepository.UpdateAsync(booking);
                return booking.Id;
            });

            var updated = await _bookingRepository.GetByIdAsync(updatedId)
                ?? throw new NotFoundException("Booking not found.");
            return updated.ToAdminDto();
        }

        public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync(string? status = null)
        {
            // Default (and anything unrecognized) shows only verified accounts -
            // unverified signups aren't "real" users yet. Admins can opt into
            // "unverified" or "all" explicitly to debug pending signups.
            bool? isEmailConfirmed = status?.ToLowerInvariant() switch
            {
                "unverified" => false,
                "all" => null,
                _ => true,
            };

            return (await _userRepository.GetAllAsync(isEmailConfirmed)).Select(u => u.ToAdminDto());
        }

        public async Task DeleteUserAsync(Guid requestingAdminId, Guid targetUserId)
        {
            if (requestingAdminId == targetUserId)
                throw new BadRequestException("You cannot delete your own account from the admin panel. Use Settings to delete your own account instead.");

            // Reuses the same hard-delete + booking/seat cleanup transaction that
            // backs self-service account deletion (Settings > Delete Account) -
            // deleting a user is the same operation regardless of who initiates it.
            await _authService.DeleteAccountAsync(targetUserId);
        }

        public async Task<IEnumerable<FlightSummaryDto>> GetAllFlightsAsync() =>
            (await _flightRepository.GetAllAsync()).Select(f => f.ToSummaryDto());

        public async Task<FlightSummaryDto> CreateFlightAsync(CreateFlightRequest request)
        {
            if (request.DepartureCityId == request.ArrivalCityId)
                throw new BadRequestException("Departure and arrival city must be different.");

            var departureCity = await _cityRepository.GetByIdAsync(request.DepartureCityId)
                ?? throw new NotFoundException("Departure city not found.");
            var arrivalCity = await _cityRepository.GetByIdAsync(request.ArrivalCityId)
                ?? throw new NotFoundException("Arrival city not found.");
            _ = await _airlineRepository.GetByIdAsync(request.AirlineId)
                ?? throw new NotFoundException("Airline not found.");

            // request.DepartureTime/ArrivalTime are the raw wall-clock digits the
            // admin typed, in the respective city's OWN local time - the client
            // sends them as-is with no timezone math (JS Intl/ICU timezone data is
            // unreliable across runtimes - it was silently producing wrong,
            // non-round offsets on-device even though the same conversion was
            // exact in Node). The server is the one place this local -> UTC
            // conversion happens, using .NET's TimeZoneInfo against each city's
            // IANA id, which has verified to be exact and consistent.
            var departureTimeUtc = ConvertCityLocalToUtc(request.DepartureTime, departureCity.TimeZoneId);
            var arrivalTimeUtc = ConvertCityLocalToUtc(request.ArrivalTime, arrivalCity.TimeZoneId);

            if (arrivalTimeUtc <= departureTimeUtc)
                throw new BadRequestException("Arrival time must be after departure time.");

            // The admin may enter the price in any supported currency for their own
            // convenience, but AZN is the only currency ever stored (see
            // ICurrencyConversionService) - other screens/reports can trust
            // Flight.BasePrice/Currency without re-checking or re-converting.
            var basePriceAzn = await _currencyConversionService.ConvertToBaseAsync(request.BasePrice, request.Currency);

            var flight = new Flight
            {
                FlightNumber = request.FlightNumber,
                AirlineId = request.AirlineId,
                DepartureCityId = request.DepartureCityId,
                DepartureTime = departureTimeUtc,
                ArrivalCityId = request.ArrivalCityId,
                ArrivalTime = arrivalTimeUtc,
                BasePrice = basePriceAzn,
                Currency = ICurrencyConversionService.BaseCurrency,
                Seats = BuildSeatMap(),
            };

            await _flightRepository.AddAsync(flight);

            var created = await _flightRepository.GetByIdAsync(flight.Id)
                ?? throw new NotFoundException("Flight not found after creation.");
            return created.ToSummaryDto();
        }

        private static DateTime ConvertCityLocalToUtc(DateTime wallClock, string ianaZoneId)
        {
            var zone = TimeZoneInfo.FindSystemTimeZoneById(ianaZoneId);
            var unspecified = DateTime.SpecifyKind(wallClock, DateTimeKind.Unspecified);
            return TimeZoneInfo.ConvertTimeToUtc(unspecified, zone);
        }

        public async Task<ExchangeRatesDto> GetExchangeRatesAsync()
        {
            var rates = await _currencyConversionService.GetRatesAsync();
            return new ExchangeRatesDto
            {
                RatesToAzn = rates.RatesToAzn.ToDictionary(r => r.Key, r => r.Value),
                FetchedAtUtc = rates.FetchedAtUtc,
                Source = rates.Source,
            };
        }

        public async Task<FlightSummaryDto> UpdateFlightPriceAsync(Guid flightId, UpdateFlightPriceRequest request)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId)
                ?? throw new NotFoundException("Flight not found.");

            var newBasePriceAzn = await _currencyConversionService.ConvertToBaseAsync(request.BasePrice, request.Currency);

            var oldPrice = flight.BasePrice;
            if (oldPrice == newBasePriceAzn)
                return flight.ToSummaryDto();

            flight.BasePrice = newBasePriceAzn;
            await _flightRepository.UpdateAsync(flight);

            // Only notify users who currently have this flight in an active (Pending)
            // booking - there's no "Saved"/favorites list in this app yet, so this is
            // the fallback the feature spec calls for. One notification per affected
            // user (not per booking), since a user could hold more than one pending
            // booking on the same flight.
            var affectedUsers = (await _bookingRepository.GetActiveBookingsByFlightIdAsync(flightId))
                .Select(b => b.User)
                .DistinctBy(u => u.Id);

            var route = $"{flight.DepartureCity.Name} -> {flight.ArrivalCity.Name}";
            var oldPriceText = $"{oldPrice.ToString("0.00", CultureInfo.InvariantCulture)} {flight.Currency}";
            var newPriceText = $"{flight.BasePrice.ToString("0.00", CultureInfo.InvariantCulture)} {flight.Currency}";

            foreach (var affectedUser in affectedUsers)
            {
                var language = affectedUser.LanguagePreference;
                await _notificationService.CreateAsync(
                    affectedUser,
                    "notifications.priceChange.title",
                    "notifications.priceChange.body",
                    new Dictionary<string, string> { ["route"] = route, ["oldPrice"] = oldPriceText, ["newPrice"] = newPriceText },
                    NotificationType.PriceChange,
                    emailSubject: _translationService.Translate(language, "email.priceChange.subject"),
                    emailHtmlBody: EmailTemplates.BuildPriceChangeEmail(_translationService, language, affectedUser.FirstName, route, oldPriceText, newPriceText));
            }

            return flight.ToSummaryDto();
        }

        public async Task<FlightSummaryDto> UpdateFlightOperationalStatusAsync(Guid flightId, UpdateFlightOperationalStatusRequest request)
        {
            var updatedFlightId = await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var flight = await _flightRepository.GetByIdAsync(flightId)
                    ?? throw new NotFoundException("Flight not found.");

                var oldGate = flight.GateNumber;
                var oldOperationalStatus = flight.OperationalStatus;

                // Same "null means leave unchanged" contract as UpdateFlightPriceRequest used to
                // have for SkyPoints. GateNumber additionally treats an empty string as "clear the gate".
                if (request.GateNumber is not null)
                    flight.GateNumber = string.IsNullOrWhiteSpace(request.GateNumber) ? null : request.GateNumber.Trim();
                if (request.OperationalStatus.HasValue)
                    flight.OperationalStatus = request.OperationalStatus.Value;

                var gateChanged = flight.GateNumber != oldGate;
                var statusChanged = flight.OperationalStatus != oldOperationalStatus;
                if (!gateChanged && !statusChanged)
                    return flight.Id;

                await _flightRepository.UpdateAsync(flight);

                // Same "only actual travelers, one notification per user" fan-out as
                // UpdateFlightPriceAsync, but against GetTravelersByFlightIdAsync
                // (Pending + Confirmed) rather than GetActiveBookingsByFlightIdAsync
                // (Pending only) - a gate change or delay matters to a Confirmed
                // traveler just as much as a Pending one.
                var travelerBookings = (await _bookingRepository.GetTravelersByFlightIdAsync(flightId)).ToList();
                var affectedUsers = travelerBookings.Select(b => b.User).DistinctBy(u => u.Id).ToList();

                if (affectedUsers.Count > 0)
                {
                    var route = $"{flight.DepartureCity.Name} -> {flight.ArrivalCity.Name}";

                    if (gateChanged && flight.GateNumber is not null)
                    {
                        var args = new Dictionary<string, string> { ["route"] = route, ["gate"] = flight.GateNumber };
                        foreach (var user in affectedUsers)
                            await _notificationService.CreateAsync(user, "notifications.gateChanged.title", "notifications.gateChanged.body", args, NotificationType.GateChanged);
                    }

                    if (statusChanged && flight.OperationalStatus == FlightOperationalStatus.Delayed)
                    {
                        var args = new Dictionary<string, string> { ["route"] = route };
                        foreach (var user in affectedUsers)
                            await _notificationService.CreateAsync(user, "notifications.flightDelayed.title", "notifications.flightDelayed.body", args, NotificationType.FlightDelayed);
                    }

                    if (statusChanged && flight.OperationalStatus == FlightOperationalStatus.Boarding)
                    {
                        var args = new Dictionary<string, string> { ["route"] = route };
                        foreach (var user in affectedUsers)
                            await _notificationService.CreateAsync(user, "notifications.boardingReminder.title", "notifications.boardingReminder.body", args, NotificationType.BoardingReminder);
                    }

                    // Unlike the three above (informational only), Cancelled actually
                    // cancels every active booking on this flight - otherwise the badge
                    // would say "Cancelled" while the system kept honoring those bookings
                    // (check-in still open, seats still held). FlightRepository.SearchAsync
                    // separately excludes Cancelled flights from search/new bookings.
                    if (statusChanged && flight.OperationalStatus == FlightOperationalStatus.Cancelled)
                    {
                        foreach (var booking in travelerBookings)
                            await CancelBookingForFlightCancellationAsync(booking.Id);

                        var args = new Dictionary<string, string> { ["route"] = route };
                        foreach (var user in affectedUsers)
                            await _notificationService.CreateAsync(user, "notifications.flightCancelled.title", "notifications.flightCancelled.body", args, NotificationType.FlightCancelled);
                    }
                }

                return flight.Id;
            });

            var updated = await _flightRepository.GetByIdAsync(updatedFlightId)
                ?? throw new NotFoundException("Flight not found.");
            return updated.ToSummaryDto();
        }

        // Mirrors BookingService.CancelAsync's seat-release + Sky Points clawback
        // exactly, minus the ownership check (admin-initiated on behalf of the
        // traveler, not the traveler's own request) and DTO return. Re-fetches the
        // booking fresh (GetTravelersByFlightIdAsync doesn't include Passengers/Seat)
        // rather than widening that shared query for its other, hotter callers
        // (price-change and check-in-open notification fan-out).
        private async Task CancelBookingForFlightCancellationAsync(Guid bookingId)
        {
            var booking = await _bookingRepository.GetByIdAsync(bookingId);
            if (booking is null || booking.Status == BookingStatus.Cancelled)
                return;

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
        }

        public async Task DeleteFlightAsync(Guid flightId)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId)
                ?? throw new NotFoundException("Flight not found.");

            if (await _bookingRepository.HasAnyForFlightAsync(flightId))
                throw new BadRequestException("Cannot delete: this flight has existing bookings.");

            await _flightRepository.DeleteAsync(flight);
        }

        private const int RevenueTrendMonths = 6;

        public async Task<AdminStatisticsDto> GetStatisticsAsync()
        {
            var statusCounts = await _bookingRepository.GetStatusCountsAsync();
            var revenueByCurrency = await _bookingRepository.GetRevenueByCurrencyAsync();
            var trend = await _bookingRepository.GetMonthlyRevenueTrendAsync(RevenueTrendMonths);
            var totalUsers = await _userRepository.CountAsync(isEmailConfirmed: true);
            var totalFlights = await _flightRepository.CountAsync();
            var totalBookings = await _bookingRepository.CountAsync();
            var totalSkyPointsIssued = await _skyPointsRepository.GetTotalAmountByTypeAsync(SkyPointsTransactionType.Earned);
            var totalSkyPointsRedeemed = await _skyPointsRepository.GetTotalAmountByTypeAsync(SkyPointsTransactionType.Redeemed);

            // Every booking is stored in a single currency today (AZN - see
            // CreateFlightAsync's ConvertToBaseAsync call), so summing the
            // per-currency dictionary is exact, not an unsafe cross-currency add;
            // this only stops being exact if Booking ever legitimately stores more
            // than one currency, at which point TotalRevenue would need real
            // conversion via ICurrencyConversionService.
            var totalRevenue = revenueByCurrency.Values.Sum();

            return new AdminStatisticsDto
            {
                TotalRevenue = totalRevenue,
                RevenueByCurrency = revenueByCurrency,
                TotalBookings = totalBookings,
                ConfirmedBookings = statusCounts.GetValueOrDefault(BookingStatus.Confirmed, 0),
                PendingBookings = statusCounts.GetValueOrDefault(BookingStatus.Pending, 0),
                CancelledBookings = statusCounts.GetValueOrDefault(BookingStatus.Cancelled, 0),
                TotalUsers = totalUsers,
                TotalFlights = totalFlights,
                TotalSkyPointsIssued = totalSkyPointsIssued,
                TotalSkyPointsRedeemed = totalSkyPointsRedeemed,
                RevenueTrend = trend
                    .Select(p => new RevenueTrendPointDto { Period = $"{p.Year:D4}-{p.Month:D2}", Revenue = p.Revenue })
                    .ToList(),
            };
        }

        private static List<SeatMap> BuildSeatMap()
        {
            var seats = new List<SeatMap>();

            foreach (var row in new[] { "A", "B", "C" })
                for (var i = 1; i <= 4; i++)
                    seats.Add(new SeatMap { SeatNumber = $"{row}{i}", Class = SeatClass.Business, PriceMultiplier = 1.8m, IsAvailable = true });

            foreach (var row in new[] { "D", "E", "F", "G" })
                for (var i = 1; i <= 6; i++)
                    seats.Add(new SeatMap { SeatNumber = $"{row}{i}", Class = SeatClass.Economy, PriceMultiplier = 1.0m, IsAvailable = true });

            return seats;
        }
    }
}
