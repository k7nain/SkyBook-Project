using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;

namespace FlyzenApi.Persistence.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly AppDbContext _context;

        public CityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetAllAsync()
        {
            return await _context.Cities.ToListAsync();
        }

        public async Task<City?> GetByIdAsync(Guid id)
        {
            return await _context.Cities.FindAsync(id);
        }

        public async Task<IEnumerable<CityGalleryImage>> GetGalleryByCityIdAsync(Guid cityId)
        {
            return await _context.CityGalleryImages.Where(x => x.CityId == cityId).ToListAsync();
        }
    }
}