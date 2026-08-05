using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class SkyPointsRepository : ISkyPointsRepository
    {
        private readonly AppDbContext _context;

        public SkyPointsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddTransactionAsync(SkyPointsTransaction transaction)
        {
            await _context.SkyPointsTransactions.AddAsync(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SkyPointsTransaction>> GetHistoryByUserIdAsync(Guid userId) =>
            await _context.SkyPointsTransactions
                .Include(t => t.RelatedBooking)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

        public async Task<int> GetEarnedAmountForBookingAsync(Guid bookingId) =>
            await _context.SkyPointsTransactions
                .Where(t => t.RelatedBookingId == bookingId && t.Type == SkyPointsTransactionType.Earned)
                .SumAsync(t => t.Amount);

        public async Task<long> GetTotalAmountByTypeAsync(SkyPointsTransactionType type) =>
            await _context.SkyPointsTransactions
                .Where(t => t.Type == type)
                .SumAsync(t => (long)t.Amount);
    }
}
