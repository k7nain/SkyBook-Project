using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class TripCountryRepository : ITripCountryRepository
    {
        private readonly AppDbContext _context;

        public TripCountryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TripCountry>> GetAllAsync() =>
            await _context.TripCountries
                .OrderBy(c => c.NameEn)
                .ToListAsync();

        public Task<TripCountry?> GetByIdAsync(Guid id) =>
            _context.TripCountries.FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(TripCountry country)
        {
            await _context.TripCountries.AddAsync(country);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(TripCountry country)
        {
            _context.TripCountries.Update(country);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TripCountry country)
        {
            _context.TripCountries.Remove(country);
            await _context.SaveChangesAsync();
        }
    }
}
