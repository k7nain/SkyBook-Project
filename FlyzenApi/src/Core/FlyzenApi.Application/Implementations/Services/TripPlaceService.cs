using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class TripPlaceService : ITripPlaceService
    {
        private readonly ITripPlaceRepository _placeRepository;
        private readonly ITripCityRepository _cityRepository;

        public TripPlaceService(ITripPlaceRepository placeRepository, ITripCityRepository cityRepository)
        {
            _placeRepository = placeRepository;
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<TripPlaceDto>> GetByCityIdAsync(Guid cityId) =>
            (await _placeRepository.GetByCityIdAsync(cityId)).Select(p => p.ToDto());

        public async Task<TripPlaceDto?> GetByIdAsync(Guid id) =>
            (await _placeRepository.GetByIdAsync(id))?.ToDto();

        public async Task<IEnumerable<TripPlaceImageDto>> GetGalleryAsync(Guid placeId) =>
            (await _placeRepository.GetGalleryByPlaceIdAsync(placeId)).Select(i => i.ToDto());

        public async Task<TripPlaceDto> CreateAsync(CreateTripPlaceRequest request)
        {
            _ = await _cityRepository.GetByIdAsync(request.CityId)
                ?? throw new NotFoundException("City not found.");

            var existing = await _placeRepository.GetByCityIdAsync(request.CityId);
            if (existing.Any(p => string.Equals(p.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A place with this name already exists in this city.");

            var place = new TripPlace
            {
                CityId = request.CityId,
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
            };

            await _placeRepository.AddAsync(place);
            return place.ToDto();
        }

        public async Task<TripPlaceDto> UpdateAsync(Guid id, UpdateTripPlaceRequest request)
        {
            var place = await _placeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Place not found.");

            place.Name = request.Name;
            place.Description = request.Description;
            place.Category = request.Category;
            place.Latitude = request.Latitude;
            place.Longitude = request.Longitude;

            await _placeRepository.UpdateAsync(place);
            return place.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var place = await _placeRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Place not found.");

            await _placeRepository.DeleteAsync(place);
        }

        public async Task<TripPlaceImageDto> AddImageAsync(Guid placeId, AddTripPlaceImageRequest request)
        {
            _ = await _placeRepository.GetByIdAsync(placeId)
                ?? throw new NotFoundException("Place not found.");

            var image = new TripPlaceImage
            {
                PlaceId = placeId,
                ImageUrl = request.ImageUrl,
            };

            await _placeRepository.AddGalleryImageAsync(image);
            return image.ToDto();
        }
    }
}
