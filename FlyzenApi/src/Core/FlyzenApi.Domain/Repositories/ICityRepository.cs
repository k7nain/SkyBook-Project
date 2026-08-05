using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ICityRepository
    {
        Task<IEnumerable<City>> GetAllAsync();
        Task<City?> GetByIdAsync(Guid id);
        // Case-insensitive exact match on the bookable City's canonical (English)
        // name - used to best-effort link a Dream Trip TripCity to a bookable
        // City for the World Map's "search flights" link, since the two are
        // separate entities with no FK between them. Returns null if there's no
        // bookable city by that name (a valid, expected outcome, not an error).
        Task<City?> GetByNameAsync(string name);
        Task<IEnumerable<CityGalleryImage>> GetGalleryByCityIdAsync(Guid cityId);
        Task UpdateAsync(City city);
        Task AddGalleryImageAsync(CityGalleryImage image);
    }
}