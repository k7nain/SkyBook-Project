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
        Task<IEnumerable<CityGalleryImage>> GetGalleryByCityIdAsync(Guid cityId);
        Task UpdateAsync(City city);
        Task AddGalleryImageAsync(CityGalleryImage image);
    }
}