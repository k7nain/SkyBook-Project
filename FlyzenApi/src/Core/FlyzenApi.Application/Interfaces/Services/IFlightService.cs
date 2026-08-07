using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IFlightService
    {
        Task<IEnumerable<FlightSummaryDto>> SearchAsync(Guid? fromCityId, Guid toCityId, DateTime? departureDate, int passengersCount);
        Task<FlightDetailDto?> GetByIdAsync(Guid id);

        // Best-effort - the "past search queries" signal for DreamTripAiService.
        // RecommendForYouAsync. userId is null for guest searches (nothing to log).
        Task LogSearchAsync(Guid? userId, Guid? fromCityId, Guid toCityId);
    }
}
