using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface IUserRecommendationCacheRepository
    {
        Task<UserRecommendationCache?> GetByUserIdAsync(Guid userId);
        Task UpsertAsync(UserRecommendationCache cache);
    }
}
