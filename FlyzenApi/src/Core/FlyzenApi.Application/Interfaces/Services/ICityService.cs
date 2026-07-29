using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ICityService
    {
        Task<IEnumerable<CityDto>> GetAllAsync();
        Task<CityDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<CityGalleryImageDto>> GetGalleryAsync(Guid cityId);
        Task<CityDto> UpdateAsync(Guid cityId, UpdateCityRequest request);
        Task<CityGalleryImageDto> AddGalleryImageAsync(Guid cityId, AddCityGalleryImageRequest request);
    }
}
