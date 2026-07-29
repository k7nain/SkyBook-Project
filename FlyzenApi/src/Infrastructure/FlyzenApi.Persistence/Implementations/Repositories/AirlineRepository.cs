using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class AirlineRepository : IAirlineRepository
    {
        private readonly AppDbContext _context;

        public AirlineRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Airline>> GetAllAsync() =>
            await _context.Airlines
                .OrderBy(a => a.Name)
                .ToListAsync();

        public Task<Airline?> GetByIdAsync(Guid id) =>
            _context.Airlines.FirstOrDefaultAsync(a => a.Id == id);

        public async Task AddAsync(Airline airline)
        {
            await _context.Airlines.AddAsync(airline);
            await _context.SaveChangesAsync();
        }
    }
}
