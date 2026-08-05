using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly AppDbContext _context;

        public CityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetAllAsync() =>
            await _context.Cities
                .OrderBy(c => c.Name)
                .ToListAsync();

        public Task<City?> GetByIdAsync(Guid id) =>
            _context.Cities.FirstOrDefaultAsync(c => c.Id == id);

        public Task<City?> GetByNameAsync(string name) =>
            _context.Cities.FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());

        public async Task<IEnumerable<CityGalleryImage>> GetGalleryByCityIdAsync(Guid cityId) =>
            await _context.CityGalleryImages
                .Where(g => g.CityId == cityId)
                .ToListAsync();

        public async Task UpdateAsync(City city)
        {
            _context.Cities.Update(city);
            await _context.SaveChangesAsync();
        }

        public async Task AddGalleryImageAsync(CityGalleryImage image)
        {
            await _context.CityGalleryImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }
    }
}
