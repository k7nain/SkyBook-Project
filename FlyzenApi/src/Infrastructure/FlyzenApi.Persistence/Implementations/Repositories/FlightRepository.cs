using FlyzenApi.Domain.Entities;
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

            return await _context.Flights
                .Include(f => f.DepartureCity)
                .Include(f => f.ArrivalCity)
                .Include(f => f.Airline)
                .Include(f => f.Seats)
                .Where(f => f.DepartureCityId == fromCityId
                            && f.ArrivalCityId == toCityId
                            && f.DepartureTime >= dayStart
                            && f.DepartureTime < dayEnd
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
    }
}
