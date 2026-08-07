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
        // Broader than GetActiveBookingsByFlightIdAsync (Pending only, used for
        // price-change notices): Pending + Confirmed, i.e. every booking that
        // represents a real traveler on this flight. Used for the operational
        // notifications (check-in, gate change, delay, boarding) that should
        // reach anyone actually flying, not just not-yet-approved bookings.
        Task<IEnumerable<Booking>> GetTravelersByFlightIdAsync(Guid flightId);
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

        // Destination Country names (City.Country, not the Dream Trip catalog)
        // ordered by how many non-cancelled bookings have gone there - the
        // "generally popular destinations" fallback for DreamTripAiService.
        // RecommendForYouAsync when a user has too little behavior history to
        // personalize for.
        Task<IReadOnlyList<string>> GetPopularDestinationCountriesAsync(int count);

        // CreatedAt of every non-cancelled booking on this flight - the raw
        // material for DynamicPricingBackgroundService's booking-velocity term
        // (recent-vs-prior booking counts). A plain timestamp list rather than
        // full Booking graphs, since that's all the formula actually needs.
        Task<IReadOnlyList<DateTime>> GetBookingCreationTimestampsByFlightIdAsync(Guid flightId);
    }
}