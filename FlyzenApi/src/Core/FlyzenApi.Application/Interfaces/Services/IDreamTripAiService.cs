using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IDreamTripAiService
    {
        Task<DreamTripRecommendResponse> RecommendAsync(Guid userId, DreamTripRecommendRequest request, CancellationToken cancellationToken = default);

        // Passive, ongoing recommendations from the user's own behavior (past
        // searches, booked/journaled destinations) rather than an active quiz -
        // see the "Sizin ucun" Home page section. Cached per-user for ~24h
        // (or until their behavior signature changes) - see UserRecommendationCache.
        Task<DreamTripRecommendResponse> RecommendForYouAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
