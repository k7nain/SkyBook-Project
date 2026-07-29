using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepository;

        public FlightService(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        public async Task<IEnumerable<FlightSummaryDto>> SearchAsync(Guid fromCityId, Guid toCityId, DateTime departureDate, int passengersCount) =>
            (await _flightRepository.SearchAsync(fromCityId, toCityId, departureDate, passengersCount))
                .Select(f => f.ToSummaryDto());

        public async Task<FlightDetailDto?> GetByIdAsync(Guid id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            return flight?.ToDetailDto();
        }
    }
}
