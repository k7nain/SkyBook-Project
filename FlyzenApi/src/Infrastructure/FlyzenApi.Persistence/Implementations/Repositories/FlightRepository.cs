using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly AppDbContext _context;

        public FlightRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Flight>> SearchAsync(Guid fromCityId, Guid toCityId, DateTime departureDate, int passengersCount)
        {
            // Npgsql maps DateTime to "timestamp with time zone" and requires Kind=Utc;
            // query-string dates arrive as Kind=Unspecified.
            var dayStart = DateTime.SpecifyKind(departureDate.Date, DateTimeKind.Utc);
            var dayEnd = dayStart.AddDays(1);
            // Booking cutoff: stop offering a flight once we're within
            // Flight.BookingCutoffHours of its departure, not just once it's
            // already departed. Evaluated fresh on every call so it reflects
            // the current time, not the search date.
            var bookingCutoff = DateTime.UtcNow.AddHours(Flight.BookingCutoffHours);

            return await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Airline)
                .Include(f => f.Seats)
                .Where(f => f.DepartureCityId == fromCityId
                            && f.ArrivalCityId == toCityId
                            && f.DepartureTime >= dayStart
                            && f.DepartureTime < dayEnd
                            && f.DepartureTime > bookingCutoff
                            && f.Seats.Count(s => s.IsAvailable) >= passengersCount)
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
