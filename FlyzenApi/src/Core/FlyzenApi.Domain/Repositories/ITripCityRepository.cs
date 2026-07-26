using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITripCityRepository
    {
        Task<IEnumerable<TripCity>> GetByCountryIdAsync(Guid countryId);
        Task<TripCity?> GetByIdAsync(Guid id);
        Task AddAsync(TripCity city);
        Task UpdateAsync(TripCity city);
        Task DeleteAsync(TripCity city);
    }
}
