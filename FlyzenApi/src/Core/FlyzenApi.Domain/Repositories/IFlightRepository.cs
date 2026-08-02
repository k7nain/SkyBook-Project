using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IFlightRepository
    {
        Task<IEnumerable<Flight>> SearchAsync(Guid fromCityId, Guid toCityId, DateTime departureDate, int passengersCount);
        Task<Flight?> GetByIdAsync(Guid id);
        Task<IEnumerable<SeatMap>> GetSeatsByFlightIdAsync(Guid flightId);
        Task<IEnumerable<Flight>> GetAllAsync();
        // Statistics-only: a plain COUNT rather than loading every Flight row
        // (with its full seat map) via GetAllAsync just to take .Count() of it.
        Task<int> CountAsync();
        // Flights not yet fully completed - the set FlightNotificationBackgroundService
        // needs to check on each run (departure/arrival reminders + status transitions).
        Task<IEnumerable<Flight>> GetActiveForNotificationCheckAsync();
        Task AddAsync(Flight flight);
        Task UpdateAsync(Flight flight);
        Task DeleteAsync(Flight flight);
    }
}