using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Repositories
{
    // One data point in a revenue-over-time series - Year/Month rather than a
    // DateTime since the aggregation itself groups by calendar month, not a
    // specific day.
    public record MonthlyRevenuePoint(int Year, int Month, decimal Revenue);

    public interface IBookingRepository
    {
        Task<Booking?> GetByIdAsync(Guid id);
        Task<IEnumerable<Booking>> GetByUserIdAsync(Guid userId);
        Task<IEnumerable<Booking>> GetAllAsync();
        Task<IEnumerable<Booking>> GetActiveBookingsByFlightIdAsync(Guid flightId);
        Task<bool> HasAnyForFlightAsync(Guid flightId);
        Task<IEnumerable<Booking>> GetBookingsDepartingOnAsync(DateTime dateUtc);
        Task AddAsync(Booking booking);
        Task UpdateAsync(Booking booking);
        Task DeleteAsync(Booking booking);

        // Statistics-only queries below - deliberately COUNT/SUM/GROUP BY at the
        // database rather than loading full Booking graphs (GetAllAsync's
        // .Include() chain) just to aggregate them in memory.
        Task<int> CountAsync();
        Task<Dictionary<BookingStatus, int>> GetStatusCountsAsync();
        // Excludes Cancelled bookings - a cancelled booking never represents
        // realized or expected revenue.
        Task<Dictionary<string, decimal>> GetRevenueByCurrencyAsync();
        Task<IReadOnlyList<MonthlyRevenuePoint>> GetMonthlyRevenueTrendAsync(int monthsBack);
    }
}