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
        Task<IEnumerable<MealOption>> GetAllMealOptionsAsync();
        Task<IEnumerable<BaggageOption>> GetAllBaggageOptionsAsync();
    }
}