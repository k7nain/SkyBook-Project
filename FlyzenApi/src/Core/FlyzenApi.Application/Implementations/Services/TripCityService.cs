using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class TripCityService : ITripCityService
    {
        private readonly ITripCityRepository _cityRepository;
        private readonly ITripCountryRepository _countryRepository;

        public TripCityService(ITripCityRepository cityRepository, ITripCountryRepository countryRepository)
        {
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
        }

        public async Task<IEnumerable<TripCityDto>> GetByCountryIdAsync(Guid countryId) =>
            (await _cityRepository.GetByCountryIdAsync(countryId)).Select(c => c.ToDto());

        public async Task<TripCityDto?> GetByIdAsync(Guid id) =>
            (await _cityRepository.GetByIdAsync(id))?.ToDto();

        public async Task<TripCityDto> CreateAsync(CreateTripCityRequest request)
        {
            _ = await _countryRepository.GetByIdAsync(request.CountryId)
                ?? throw new NotFoundException("Country not found.");

            var existing = await _cityRepository.GetByCountryIdAsync(request.CountryId);
            if (existing.Any(c => string.Equals(c.Name, request.Name, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A city with this name already exists in this country.");

            var city = new TripCity
            {
                CountryId = request.CountryId,
                Name = request.Name,
                Image = request.Image,
                ShortDescription = request.ShortDescription,
            };

            await _cityRepository.AddAsync(city);
            return city.ToDto();
        }

        public async Task<TripCityDto> UpdateAsync(Guid id, UpdateTripCityRequest request)
        {
            var city = await _cityRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("City not found.");

            city.Name = request.Name;
            city.Image = request.Image;
            city.ShortDescription = request.ShortDescription;

            await _cityRepository.UpdateAsync(city);
            return city.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var city = await _cityRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("City not found.");

            await _cityRepository.DeleteAsync(city);
        }
    }
}
