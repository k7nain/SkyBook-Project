using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        // "Any origin" mode (fromCityId omitted, e.g. a World Map marker click
        // with no origin/date the visiting user has chosen yet) can't match a
        // single day the way a normal from/to/date search does, so it shows a
        // rolling window of upcoming flights instead.
        private const int AnyOriginWindowDays = 60;

        private readonly AppDbContext _context;

        public FlightRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flight>> SearchAsync(Guid? fromCityId, Guid toCityId, DateTime? departureDate, int passengersCount)
        {
            // Booking cutoff: stop offering a flight once we're within
            // Flight.BookingCutoffHours of its departure, not just once it's
            // already departed. Evaluated fresh on every call so it reflects
            // the current time, not the search date.
            var bookingCutoff = DateTime.UtcNow.AddHours(Flight.BookingCutoffHours);

            var query = _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Airline)
                .Include(f => f.Seats)
                .Where(f => f.ArrivalCityId == toCityId
                            && f.DepartureTime > bookingCutoff
                            && f.Seats.Count(s => s.IsAvailable) >= passengersCount);

            // Npgsql maps DateTime to "timestamp with time zone" and requires Kind=Utc;
            // query-string dates arrive as Kind=Unspecified. Service layer
            // guarantees departureDate is present whenever fromCityId is.
            if (fromCityId.HasValue)
            {
                var dayStart = DateTime.SpecifyKind(departureDate!.Value.Date, DateTimeKind.Utc);
                var dayEnd = dayStart.AddDays(1);
                query = query.Where(f => f.DepartureCityId == fromCityId.Value && f.DepartureTime >= dayStart && f.DepartureTime < dayEnd);
            }
            else
            {
                var windowStart = departureDate.HasValue
                    ? DateTime.SpecifyKind(departureDate.Value.Date, DateTimeKind.Utc)
                    : DateTime.UtcNow.Date;
                var windowEnd = windowStart.AddDays(AnyOriginWindowDays);
                query = query.Where(f => f.DepartureTime >= windowStart && f.DepartureTime < windowEnd);
            }

            return await query
                .OrderBy(f => f.DepartureTime)
                .ToListAsync();
        }

        public Task<Flight?> GetByIdAsync(Guid id) =>
            _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Airline)
                .Include(f => f.Seats)
                .FirstOrDefaultAsync(f => f.Id == id);

        public async Task<IEnumerable<SeatMap>> GetSeatsByFlightIdAsync(Guid flightId) =>
            await _context.Seats
                .Where(s => s.FlightId == flightId)
                .OrderBy(s => s.SeatNumber)
                .ToListAsync();

        public async Task<IEnumerable<Flight>> GetAllAsync() =>
            await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Airline)
                .Include(f => f.Seats)
                .OrderByDescending(f => f.DepartureTime)
                .ToListAsync();

        public Task<int> CountAsync() => _context.Flights.CountAsync();

        public Task<int> CountByStatusAsync(FlightStatus status) =>
            _context.Flights.CountAsync(f => f.Status == status);

        public async Task<int> CountDistinctDestinationCitiesAsync()
        {
            var departureCityIds = _context.Flights.Select(f => f.DepartureCityId);
            var arrivalCityIds = _context.Flights.Select(f => f.ArrivalCityId);
            return await departureCityIds.Union(arrivalCityIds).CountAsync();
        }

        public async Task<IEnumerable<Flight>> GetActiveForNotificationCheckAsync() =>
            await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Where(f => f.Status != FlightStatus.Completed)
                .ToListAsync();

        public async Task AddAsync(Flight flight)
        {
            await _context.Flights.AddAsync(flight);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Flight flight)
        {
            _context.Flights.Update(flight);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Flight flight)
        {
            // Seats and FlightNotificationLogs cascade-delete via FK config
            // (AppDbContext); Bookings are Restrict, so this throws at the DB
            // level if any exist - callers should check HasAnyForFlightAsync
            // first to surface a clean error instead.
            _context.Flights.Remove(flight);
            await _context.SaveChangesAsync();
        }
    }
}
