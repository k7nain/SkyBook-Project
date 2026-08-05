using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class SkyPointsService : ISkyPointsService
    {
        private readonly ISkyPointsRepository _skyPointsRepository;

        public SkyPointsService(ISkyPointsRepository skyPointsRepository)
        {
            _skyPointsRepository = skyPointsRepository;
        }

        public async Task<IEnumerable<SkyPointsTransactionDto>> GetHistoryAsync(Guid userId) =>
            (await _skyPointsRepository.GetHistoryByUserIdAsync(userId)).Select(t => t.ToDto());
    }
}
