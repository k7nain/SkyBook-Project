using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ISearchLogRepository
    {
        Task AddAsync(SearchLog log);
        Task<IEnumerable<SearchLog>> GetRecentByUserIdAsync(Guid userId, int days);
    }
}
