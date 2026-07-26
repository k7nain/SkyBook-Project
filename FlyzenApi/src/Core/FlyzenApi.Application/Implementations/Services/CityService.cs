using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;

        public CityService(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<CityDto>> GetAllAsync() =>
            (await _cityRepository.GetAllAsync()).Select(c => c.ToDto());

        public async Task<CityDto?> GetByIdAsync(Guid id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            return city?.ToDto();
        }

        public async Task<IEnumerable<CityGalleryImageDto>> GetGalleryAsync(Guid cityId) =>
            (await _cityRepository.GetGalleryByCityIdAsync(cityId)).Select(g => g.ToDto());

        public async Task<CityDto> UpdateAsync(Guid cityId, UpdateCityRequest request)
        {
            var city = await _cityRepository.GetByIdAsync(cityId)
                ?? throw new NotFoundException("City not found.");

            if (request.Description is not null)
                city.Description = request.Description;
            if (request.Climate is not null)
                city.Climate = request.Climate;
            if (request.Attractions is not null)
                city.Attractions = request.Attractions;

            await _cityRepository.UpdateAsync(city);
            return city.ToDto();
        }

        public async Task<CityGalleryImageDto> AddGalleryImageAsync(Guid cityId, AddCityGalleryImageRequest request)
        {
            _ = await _cityRepository.GetByIdAsync(cityId)
                ?? throw new NotFoundException("City not found.");

            var image = new CityGalleryImage
            {
                CityId = cityId,
                ImageUrl = request.ImageUrl,
                Description = request.Description,
            };

            await _cityRepository.AddGalleryImageAsync(image);
            return image.ToDto();
        }
    }
}
