using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class FlightService : IFlightService
    {
        private readonly IFlightRepository _flightRepository;
        private readonly ISearchLogRepository _searchLogRepository;

        public FlightService(IFlightRepository flightRepository, ISearchLogRepository searchLogRepository)
        {
            _flightRepository = flightRepository;
            _searchLogRepository = searchLogRepository;
        }

        public async Task<IEnumerable<FlightSummaryDto>> SearchAsync(Guid? fromCityId, Guid toCityId, DateTime? departureDate, int passengersCount)
        {
            // departureDate is only mandatory for a normal from/to/date search -
            // "any origin" mode (fromCityId omitted) defaults it to a rolling
            // window in the repository instead (see FlightRepository.SearchAsync).
            if (fromCityId.HasValue && departureDate is null)
            {
                throw new BadRequestException("departureDate is required when fromCityId is specified.");
            }

            return (await _flightRepository.SearchAsync(fromCityId, toCityId, departureDate, passengersCount))
                .Select(f => f.ToSummaryDto());
        }

        public async Task<FlightDetailDto?> GetByIdAsync(Guid id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            return flight?.ToDetailDto();
        }

        public async Task LogSearchAsync(Guid? userId, Guid? fromCityId, Guid toCityId)
        {
            if (!userId.HasValue)
                return;

            await _searchLogRepository.AddAsync(new SearchLog
            {
                UserId = userId.Value,
                FromCityId = fromCityId,
                ToCityId = toCityId,
            });
        }
    }
}
