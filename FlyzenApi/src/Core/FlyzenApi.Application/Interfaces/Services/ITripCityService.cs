using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITripCityService
    {
        Task<IEnumerable<TripCityDto>> GetByCountryIdAsync(Guid countryId);
        Task<TripCityDto?> GetByIdAsync(Guid id);
        Task<TripCityDto> CreateAsync(CreateTripCityRequest request);
        Task<TripCityDto> UpdateAsync(Guid id, UpdateTripCityRequest request);
        Task DeleteAsync(Guid id);
    }
}
