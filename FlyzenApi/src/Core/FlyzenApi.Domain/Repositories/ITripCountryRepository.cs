using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITripCountryRepository
    {
        Task<IEnumerable<TripCountry>> GetAllAsync();
        Task<TripCountry?> GetByIdAsync(Guid id);
        Task AddAsync(TripCountry country);
        Task UpdateAsync(TripCountry country);
        Task DeleteAsync(TripCountry country);
    }
}
