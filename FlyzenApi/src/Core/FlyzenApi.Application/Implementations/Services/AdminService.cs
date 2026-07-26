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

        public AdminService(
            IBookingRepository bookingRepository,
            IUserRepository userRepository,
            IFlightRepository flightRepository,
            ICityRepository cityRepository,
            IAirlineRepository airlineRepository,
            IUnitOfWork unitOfWork,
            IAuthService authService,
            INotificationService notificationService)
        {
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _flightRepository = flightRepository;
            _cityRepository = cityRepository;
            _airlineRepository = airlineRepository;
            _unitOfWork = unitOfWork;
            _authService = authService;
            _notificationService = notificationService;
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
            if (request.ArrivalTime <= request.DepartureTime)
                throw new BadRequestException("Arrival time must be after departure time.");

            _ = await _cityRepository.GetByIdAsync(request.DepartureCityId)
                ?? throw new NotFoundException("Departure city not found.");
            _ = await _cityRepository.GetByIdAsync(request.ArrivalCityId)
                ?? throw new NotFoundException("Arrival city not found.");
            _ = await _airlineRepository.GetByIdAsync(request.AirlineId)
                ?? throw new NotFoundException("Airline not found.");

            var flight = new Flight
            {
                FlightNumber = request.FlightNumber,
                AirlineId = request.AirlineId,
                DepartureCityId = request.DepartureCityId,
                DepartureTime = DateTime.SpecifyKind(request.DepartureTime, DateTimeKind.Utc),
                ArrivalCityId = request.ArrivalCityId,
                ArrivalTime = DateTime.SpecifyKind(request.ArrivalTime, DateTimeKind.Utc),
                BasePrice = request.BasePrice,
                Seats = BuildSeatMap(),
            };

            await _flightRepository.AddAsync(flight);

            var created = await _flightRepository.GetByIdAsync(flight.Id)
                ?? throw new NotFoundException("Flight not found after creation.");
            return created.ToSummaryDto();
        }

        public async Task<FlightSummaryDto> UpdateFlightPriceAsync(Guid flightId, UpdateFlightPriceRequest request)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId)
                ?? throw new NotFoundException("Flight not found.");

            var oldPrice = flight.BasePrice;
            if (oldPrice == request.BasePrice)
                return flight.ToSummaryDto();

            flight.BasePrice = request.BasePrice;
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
                await _notificationService.CreateAsync(
                    affectedUser.Id,
                    affectedUser.Email,
                    "Price update for your pending booking",
                    $"The price for {route} changed from {oldPriceText} to {newPriceText}.",
                    NotificationType.PriceChange,
                    emailSubject: "Price update for your pending booking",
                    emailHtmlBody: EmailTemplates.BuildPriceChangeEmail(affectedUser.FirstName, route, oldPriceText, newPriceText));
            }

            return flight.ToSummaryDto();
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
