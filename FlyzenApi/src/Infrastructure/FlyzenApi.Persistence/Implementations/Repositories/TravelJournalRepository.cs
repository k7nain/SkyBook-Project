using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class TravelJournalRepository : ITravelJournalRepository
    {
        private readonly AppDbContext _context;

        public TravelJournalRepository(AppDbContext context)
        {
            _context = context;
        }

        private IQueryable<TravelJournal> WithIncludes() =>
            _context.TravelJournals
                .Include(j => j.User)
                .Include(j => j.DestinationCity)
                .Include(j => j.Images.OrderBy(i => i.DisplayOrder));

        public Task<TravelJournal?> GetByIdAsync(Guid id) =>
            WithIncludes().FirstOrDefaultAsync(j => j.Id == id);

        public async Task<IEnumerable<TravelJournal>> GetPublishedByCityIdAsync(Guid cityId) =>
            await WithIncludes()
                .Where(j => j.DestinationCityId == cityId && j.IsPublished)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task<IEnumerable<TravelJournal>> GetByUserIdAsync(Guid userId) =>
            await WithIncludes()
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();

        public async Task AddAsync(TravelJournal journal)
        {
            await _context.TravelJournals.AddAsync(journal);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TravelJournal journal)
        {
            _context.TravelJournals.Remove(journal);
            await _context.SaveChangesAsync();
        }
    }
}
