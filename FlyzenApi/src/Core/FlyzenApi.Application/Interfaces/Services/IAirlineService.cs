using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IAirlineService
    {
        Task<IEnumerable<AirlineDto>> GetAllAsync();
        Task<AirlineDto> CreateAsync(CreateAirlineRequest request);
    }
}
