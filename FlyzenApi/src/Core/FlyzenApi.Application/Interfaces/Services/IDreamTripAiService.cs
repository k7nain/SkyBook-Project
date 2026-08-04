using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IDreamTripAiService
    {
        Task<DreamTripRecommendResponse> RecommendAsync(Guid userId, DreamTripRecommendRequest request, CancellationToken cancellationToken = default);
    }
}
