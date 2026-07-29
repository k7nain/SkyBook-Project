using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class BaggageOptionRepository : IBaggageOptionRepository
    {
        private readonly AppDbContext _context;

        public BaggageOptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BaggageOption>> GetAllAsync() =>
            await _context.BaggageOptions.OrderBy(o => o.DisplayOrder).ToListAsync();

        public Task<BaggageOption?> GetByIdAsync(Guid id) =>
            _context.BaggageOptions.FirstOrDefaultAsync(o => o.Id == id);
    }
}
