using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Domain.Repositories
{
    public interface ITravelJournalRepository
    {
        Task<TravelJournal?> GetByIdAsync(Guid id);
        Task<IEnumerable<TravelJournal>> GetPublishedByCityIdAsync(Guid cityId);
        Task<IEnumerable<TravelJournal>> GetByUserIdAsync(Guid userId);
        Task AddAsync(TravelJournal journal);
        Task DeleteAsync(TravelJournal journal);
    }
}
