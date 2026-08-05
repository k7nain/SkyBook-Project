using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IPublicStatsService
    {
        Task<PublicStatsDto> GetStatsAsync(CancellationToken cancellationToken = default);
    }
}
