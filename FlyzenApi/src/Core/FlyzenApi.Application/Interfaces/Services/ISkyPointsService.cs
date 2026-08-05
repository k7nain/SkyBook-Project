using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ISkyPointsService
    {
        Task<IEnumerable<SkyPointsTransactionDto>> GetHistoryAsync(Guid userId);
    }
}
