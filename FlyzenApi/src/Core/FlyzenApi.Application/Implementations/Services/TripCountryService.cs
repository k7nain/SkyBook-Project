using System.Text.RegularExpressions;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class TripCountryService : ITripCountryService
    {
        private readonly ITripCountryRepository _countryRepository;

        public TripCountryService(ITripCountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<IEnumerable<TripCountryDto>> GetAllAsync() =>
            (await _countryRepository.GetAllAsync()).Select(c => c.ToDto());

        public async Task<TripCountryDto?> GetByIdAsync(Guid id) =>
            (await _countryRepository.GetByIdAsync(id))?.ToDto();

        public async Task<TripCountryDto> CreateAsync(CreateTripCountryRequest request)
        {
            var flagCode = ValidateFlagCode(request.FlagCode);

            // Keyed on NameAz, not NameEn - NameEn is now filled asynchronously by
            // auto-translate and can be null for multiple rows at once (pending
            // translation), which would make an EN-based check either miss real
            // duplicates or spuriously flag unrelated countries as conflicting.
            var existing = await _countryRepository.GetAllAsync();
            if (existing.Any(c => string.Equals(c.NameAz, request.NameAz, StringComparison.OrdinalIgnoreCase)))
                throw new ConflictException("A country with this name already exists.");

            var country = new TripCountry
            {
                NameAz = request.NameAz,
                NameEn = request.NameEn,
                NameRu = request.NameRu,
                FlagCode = flagCode,
                CoverImage = request.CoverImage,
            };

            await _countryRepository.AddAsync(country);
            return country.ToDto();
        }

        public async Task<TripCountryDto> UpdateAsync(Guid id, UpdateTripCountryRequest request)
        {
            var country = await _countryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Country not found.");

            country.NameAz = request.NameAz;
            country.NameEn = request.NameEn;
            country.NameRu = request.NameRu;
            country.FlagCode = ValidateFlagCode(request.FlagCode);
            country.CoverImage = request.CoverImage;

            await _countryRepository.UpdateAsync(country);
            return country.ToDto();
        }

        public async Task DeleteAsync(Guid id)
        {
            var country = await _countryRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Country not found.");

            await _countryRepository.DeleteAsync(country);
        }

        private static string ValidateFlagCode(string flagCode)
        {
            if (!Regex.IsMatch(flagCode, "^[A-Za-z]{2}$"))
                throw new BadRequestException("Flag code must be exactly 2 letters (ISO 3166-1 alpha-2).");

            return flagCode.ToUpperInvariant();
        }
    }
}
