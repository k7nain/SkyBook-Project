using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class SearchLogRepository : ISearchLogRepository
    {
        private readonly AppDbContext _context;

        public SearchLogRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SearchLog log)
        {
            await _context.SearchLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SearchLog>> GetRecentByUserIdAsync(Guid userId, int days)
        {
            var cutoff = DateTime.UtcNow.AddDays(-days);
            return await _context.SearchLogs
                .Include(l => l.ToCity)
                .Where(l => l.UserId == userId && l.CreatedAt >= cutoff)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }
    }
}
