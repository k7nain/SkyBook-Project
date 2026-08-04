using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITripCountryRepository
    {
        Task<IEnumerable<TripCountry>> GetAllAsync();
        // Includes Cities and their Places (with Category) - used by the AI Dream Trip
        // generator to build a catalog it can reason over without invoking per-country
        // extra queries.
        Task<IEnumerable<TripCountry>> GetAllWithCitiesAndPlacesAsync();
        Task<TripCountry?> GetByIdAsync(Guid id);
        Task AddAsync(TripCountry country);
        Task UpdateAsync(TripCountry country);
        Task DeleteAsync(TripCountry country);
    }
}
