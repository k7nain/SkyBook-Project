using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightSummaryDto>> SearchAsync(Guid? fromCityId, Guid toCityId, DateTime? departureDate, int passengersCount);
        Task<FlightDetailDto?> GetByIdAsync(Guid id);
    }
}
