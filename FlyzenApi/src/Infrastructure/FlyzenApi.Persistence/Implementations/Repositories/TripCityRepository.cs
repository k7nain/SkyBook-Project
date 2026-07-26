using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class TripCityRepository : ITripCityRepository
    {
        private readonly AppDbContext _context;

        public TripCityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TripCity>> GetByCountryIdAsync(Guid countryId) =>
            await _context.TripCities
                .Where(c => c.CountryId == countryId)
                .OrderBy(c => c.Name)
                .ToListAsync();

        public Task<TripCity?> GetByIdAsync(Guid id) =>
            _context.TripCities.FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(TripCity city)
        {
            await _context.TripCities.AddAsync(city);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripCity city)
        {
            _context.TripCities.Update(city);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TripCity city)
        {
            _context.TripCities.Remove(city);
            await _context.SaveChangesAsync();
        }
    }
}
