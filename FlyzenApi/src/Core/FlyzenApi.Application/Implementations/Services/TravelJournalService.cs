using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class TravelJournalService : ITravelJournalService
    {
        private readonly ITravelJournalRepository _journalRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ICityRepository _cityRepository;
        private readonly ITripCityRepository _tripCityRepository;

        public TravelJournalService(
            ITravelJournalRepository journalRepository,
            IBookingRepository bookingRepository,
            ICityRepository cityRepository,
            ITripCityRepository tripCityRepository)
        {
            _journalRepository = journalRepository;
            _bookingRepository = bookingRepository;
            _cityRepository = cityRepository;
            _tripCityRepository = tripCityRepository;
        }

        public async Task<TravelJournalDto> CreateAsync(Guid userId, CreateTravelJournalRequest request)
        {
            var booking = await _bookingRepository.GetByIdAsync(request.BookingId)
                ?? throw new NotFoundException("Booking not found.");

            if (booking.UserId != userId)
                throw new NotFoundException("Booking not found.");

            // "Verified traveler" eligibility: a real, non-cancelled booking on a
            // flight that has actually completed (FlightNotificationBackgroundService
            // flips Flight.Status to Completed once ArrivalTime passes) - not a
            // separate BookingStatus.Completed value, since this app's bookings
            // only ever move between Pending/Confirmed/Cancelled (see AdminService.
            // UpdateBookingStatusAsync) and never a "Completed" status of their own.
            if (booking.Status == BookingStatus.Cancelled)
                throw new BadRequestException("You can only share a journal for a trip you actually took.");
            if (booking.Flight.Status != FlightStatus.Completed)
                throw new BadRequestException("You can only share a journal once this trip has been completed.");

            var journal = new TravelJournal
            {
                UserId = userId,
                BookingId = booking.Id,
                DestinationCityId = booking.Flight.ArrivalCityId,
                Title = request.Title.Trim(),
                Body = request.Body.Trim(),
                Rating = request.Rating,
                Images = request.ImageUrls
                    .Select((url, index) => new TravelJournalImage { ImageUrl = url, DisplayOrder = index })
                    .ToList(),
            };

            await _journalRepository.AddAsync(journal);

            var created = await _journalRepository.GetByIdAsync(journal.Id)
                ?? throw new NotFoundException("Journal not found after creation.");
            return created.ToDto();
        }

        public async Task<IEnumerable<TravelJournalDto>> GetByCityIdAsync(Guid cityId) =>
            (await _journalRepository.GetPublishedByCityIdAsync(cityId)).Select(j => j.ToDto());

        public async Task<IEnumerable<TravelJournalDto>> GetByTripCityIdAsync(Guid tripCityId)
        {
            var tripCity = await _tripCityRepository.GetByIdAsync(tripCityId);
            var name = tripCity?.NameEn ?? tripCity?.NameAz;
            if (string.IsNullOrWhiteSpace(name))
                return Enumerable.Empty<TravelJournalDto>();

            // Best-effort link, same reasoning/pattern as WorldMapService's
            // TripPlace -> bookable City resolution: the Dream Trip catalog and
            // the bookable City table are separate entities with no FK between
            // them, matched only by name. No match is a valid, expected outcome.
            var bookableCity = await _cityRepository.GetByNameAsync(name);
            if (bookableCity is null)
                return Enumerable.Empty<TravelJournalDto>();

            return await GetByCityIdAsync(bookableCity.Id);
        }

        public async Task<IEnumerable<TravelJournalDto>> GetMineAsync(Guid userId) =>
            (await _journalRepository.GetByUserIdAsync(userId)).Select(j => j.ToDto());

        public async Task DeleteAsync(Guid journalId, Guid userId)
        {
            var journal = await _journalRepository.GetByIdAsync(journalId)
                ?? throw new NotFoundException("Journal not found.");
            if (journal.UserId != userId)
                throw new NotFoundException("Journal not found.");

            await _journalRepository.DeleteAsync(journal);
        }
    }
}
