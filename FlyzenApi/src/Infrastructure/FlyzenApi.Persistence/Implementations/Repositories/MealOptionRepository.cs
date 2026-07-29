using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class MealOptionRepository : IMealOptionRepository
    {
        private readonly AppDbContext _context;

        public MealOptionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<MealOption>> GetAllAsync() =>
            // Cheapest first, so "No Meal" (always priced 0) reliably sorts to the top -
            // the booking screen defaults its selection to whichever option loads first.
            await _context.MealOptions.OrderBy(m => m.Price).ToListAsync();

        public Task<MealOption?> GetByIdAsync(Guid id) =>
            _context.MealOptions.FirstOrDefaultAsync(m => m.Id == id);
    }
}
