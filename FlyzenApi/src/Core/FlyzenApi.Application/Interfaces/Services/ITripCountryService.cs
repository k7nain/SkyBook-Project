using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITripCountryService
    {
        Task<IEnumerable<TripCountryDto>> GetAllAsync();
        Task<TripCountryDto?> GetByIdAsync(Guid id);
        Task<TripCountryDto> CreateAsync(CreateTripCountryRequest request);
        Task<TripCountryDto> UpdateAsync(Guid id, UpdateTripCountryRequest request);
        Task DeleteAsync(Guid id);
    }
}
