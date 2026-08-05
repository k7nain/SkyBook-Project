using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IWorldMapService
    {
        Task<IEnumerable<MapMarkerDto>> GetMarkersInBoundsAsync(MapBoundsRequest request);
        Task<DestinationDetailDto> GetDestinationDetailAsync(Guid id, string? lang);
        Task<IEnumerable<MapMarkerDto>> GetUserRecommendedMarkersAsync(Guid userId, string? lang);
    }
}
