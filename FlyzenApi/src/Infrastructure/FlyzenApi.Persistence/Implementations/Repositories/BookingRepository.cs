using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<Booking?> GetByIdAsync(Guid id) =>
            _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.DepartureCity)
                .Include(b => b.Flight).ThenInclude(f => f.ArrivalCity)
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Flight).ThenInclude(f => f.Seats)
                .Include(b => b.Passengers).ThenInclude(p => p.Seat)
                .Include(b => b.Passengers).ThenInclude(p => p.MealOption)
                .Include(b => b.Passengers).ThenInclude(p => p.BaggageOption)
                .Include(b => b.Ticket)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == id);

        public async Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId) =>
            await _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.DepartureCity)
                .Include(b => b.Flight).ThenInclude(f => f.ArrivalCity)
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Flight).ThenInclude(f => f.Seats)
                .Include(b => b.Passengers).ThenInclude(p => p.Seat)
                .Include(b => b.Ticket)
                .Where(b => b.UserId == userId && !b.IsDeleted)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetAllAsync() =>
            await _context.Bookings
                .Include(b => b.Flight).ThenInclude(f => f.DepartureCity)
                .Include(b => b.Flight).ThenInclude(f => f.ArrivalCity)
                .Include(b => b.Flight).ThenInclude(f => f.Airline)
                .Include(b => b.Flight).ThenInclude(f => f.Seats)
                .Include(b => b.Passengers).ThenInclude(p => p.Seat)
                .Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<Booking>> GetActiveBookingsByFlightIdAsync(Guid flightId) =>
            await _context.Bookings
                .Include(b => b.User)
                .Where(b => b.FlightId == flightId && b.Status == BookingStatus.Pending)
                .ToListAsync();

        // Any booking at all (regardless of status) - used to decide whether a
        // flight is safe to hard-delete. The Booking->Flight FK is Restrict, so
        // this check exists to fail with a clear message instead of a raw DB
        // constraint error.
        public Task<bool> HasAnyForFlightAsync(Guid flightId) =>
            _context.Bookings.AnyAsync(b => b.FlightId == flightId);

        public async Task<IEnumerable<Booking>> GetBookingsDepartingOnAsync(DateTime dateUtc)
        {
            var start = dateUtc.Date;
            var end = start.AddDays(1);

            return await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Flight).ThenInclude(f => f.DepartureCity)
                .Include(b => b.Flight).ThenInclude(f => f.ArrivalCity)
                .Where(b => b.Status != BookingStatus.Cancelled
                    && b.Flight.DepartureTime >= start
                    && b.Flight.DepartureTime < end)
                .ToListAsync();
        }

        public async Task AddAsync(Booking booking)
        {
            await _context.Bookings.AddAsync(booking);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Booking booking)
        {
            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Booking booking)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }

        public Task<int> CountAsync() => _context.Bookings.CountAsync();

        public async Task<Dictionary<BookingStatus, int>> GetStatusCountsAsync()
        {
            var counts = await _context.Bookings
                .GroupBy(b => b.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return counts.ToDictionary(c => c.Status, c => c.Count);
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByCurrencyAsync()
        {
            var totals = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled)
                .GroupBy(b => b.Currency)
                .Select(g => new { Currency = g.Key, Total = g.Sum(b => b.TotalPrice) })
                .ToListAsync();

            return totals.ToDictionary(t => t.Currency, t => t.Total);
        }

        public async Task<IReadOnlyList<MonthlyRevenuePoint>> GetMonthlyRevenueTrendAsync(int monthsBack)
        {
            var cutoff = DateTime.UtcNow.Date.AddMonths(-monthsBack + 1);
            cutoff = new DateTime(cutoff.Year, cutoff.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var points = await _context.Bookings
                .Where(b => b.Status != BookingStatus.Cancelled && b.CreatedAt >= cutoff)
                .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
                .Select(g => new MonthlyRevenuePoint(g.Key.Year, g.Key.Month, g.Sum(b => b.TotalPrice)))
                .ToListAsync();

            return points.OrderBy(p => p.Year).ThenBy(p => p.Month).ToList();
        }
    }
}
