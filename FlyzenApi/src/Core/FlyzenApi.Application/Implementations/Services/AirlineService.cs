using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class AirlineService : IAirlineService
    {
        private readonly IAirlineRepository _airlineRepository;

        public AirlineService(IAirlineRepository airlineRepository)
        {
            _airlineRepository = airlineRepository;
        }

        public async Task<IEnumerable<AirlineDto>> GetAllAsync() =>
            (await _airlineRepository.GetAllAsync()).Select(a => a.ToDto());

        public async Task<AirlineDto> CreateAsync(CreateAirlineRequest request)
        {
            var existing = await _airlineRepository.GetAllAsync();
            if (existing.Any(a => string.Equals(a.Code, request.Code, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("An airline with this code already exists.");

            var airline = new Airline
            {
                Name = request.Name,
                Code = request.Code.ToUpperInvariant(),
            };

            await _airlineRepository.AddAsync(airline);
            return airline.ToDto();
        }
    }
}
