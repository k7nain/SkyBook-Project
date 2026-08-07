using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITravelJournalService
    {
        Task<TravelJournalDto> CreateAsync(Guid userId, CreateTravelJournalRequest request);
        Task<IEnumerable<TravelJournalDto>> GetByCityIdAsync(Guid cityId);
        Task<IEnumerable<TravelJournalDto>> GetByTripCityIdAsync(Guid tripCityId);
        Task<IEnumerable<TravelJournalDto>> GetMineAsync(Guid userId);
        Task DeleteAsync(Guid journalId, Guid userId);
    }
}
