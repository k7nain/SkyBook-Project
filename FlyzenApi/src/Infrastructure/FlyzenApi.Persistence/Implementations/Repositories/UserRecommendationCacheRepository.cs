using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.Implementations.Repositories
{
    public class UserRecommendationCacheRepository : IUserRecommendationCacheRepository
    {
        private readonly AppDbContext _context;

        public UserRecommendationCacheRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<UserRecommendationCache?> GetByUserIdAsync(Guid userId) =>
            _context.UserRecommendationCaches.FirstOrDefaultAsync(c => c.UserId == userId);

        public async Task UpsertAsync(UserRecommendationCache cache)
        {
            var existing = await _context.UserRecommendationCaches.FirstOrDefaultAsync(c => c.UserId == cache.UserId);
            if (existing is null)
            {
                await _context.UserRecommendationCaches.AddAsync(cache);
            }
            else
            {
                existing.ResponseJson = cache.ResponseJson;
                existing.SignatureHash = cache.SignatureHash;
                existing.GeneratedAt = cache.GeneratedAt;
            }
            await _context.SaveChangesAsync();
        }
    }
}
