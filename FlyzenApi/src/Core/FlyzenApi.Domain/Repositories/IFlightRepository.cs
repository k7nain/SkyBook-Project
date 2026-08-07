using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Repositories
{
    public interface IFlightRepository
    {
        // fromCityId null => "any origin" (e.g. World Map marker click) - see
        // FlightRepository.SearchAsync for how the date filter differs in that mode.
        Task<IEnumerable<Flight>> SearchAsync(Guid? fromCityId, Guid toCityId, DateTime? departureDate, int passengersCount);
        Task<Flight?> GetByIdAsync(Guid id);
        Task<IEnumerable<SeatMap>> GetSeatsByFlightIdAsync(Guid flightId);
        Task<IEnumerable<Flight>> GetAllAsync();
        // Statistics-only: a plain COUNT rather than loading every Flight row
        // (with its full seat map) via GetAllAsync just to take .Count() of it.
        Task<int> CountAsync();
        Task<int> CountByStatusAsync(FlightStatus status);
        // Distinct union of DepartureCityId/ArrivalCityId across all flights -
        // "how many bookable cities does the app actually fly to/from", used
        // by the public About-page "Destinations" stat.
        Task<int> CountDistinctDestinationCitiesAsync();
        // Flights not yet fully completed - the set FlightNotificationBackgroundService
        // needs to check on each run (departure/arrival reminders + status transitions).
        Task<IEnumerable<Flight>> GetActiveForNotificationCheckAsync();
        // Flights DynamicPricingBackgroundService should evaluate: Scheduled only
        // (a Departed/Completed flight can no longer be booked, so proposing a new
        // price for it is meaningless) - includes Seats, needed for the occupancy
        // term of the demand formula.
        Task<IEnumerable<Flight>> GetActiveForPricingCheckAsync();
        Task AddAsync(Flight flight);
        Task UpdateAsync(Flight flight);
        Task DeleteAsync(Flight flight);
    }
}