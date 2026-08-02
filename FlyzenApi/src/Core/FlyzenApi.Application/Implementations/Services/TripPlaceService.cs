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

            // Keyed on NameAz - see TripCountryService.CreateAsync for why (NameEn is
            // now filled asynchronously and can be null for multiple rows at once).
            var existing = await _placeRepository.GetByCityIdAsync(request.CityId);
            if (existing.Any(p => string.Equals(p.NameAz, request.NameAz, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A place with this name already exists in this city.");

            var place = new TripPlace
            {
                CityId = request.CityId,
                NameAz = request.NameAz,
                NameEn = request.NameEn,
                NameRu = request.NameRu,
                DescriptionAz = request.DescriptionAz,
                DescriptionEn = request.DescriptionEn,
                DescriptionRu = request.DescriptionRu,
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

            place.NameAz = request.NameAz;
            place.NameEn = request.NameEn;
            place.NameRu = request.NameRu;
            place.DescriptionAz = request.DescriptionAz;
            place.DescriptionEn = request.DescriptionEn;
            place.DescriptionRu = request.DescriptionRu;
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
