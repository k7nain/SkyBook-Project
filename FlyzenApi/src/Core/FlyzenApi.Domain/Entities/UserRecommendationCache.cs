using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    // One row per user - the cached result of DreamTripAiService.RecommendForYouAsync,
    // so the OpenRouter call only happens once a day per user (or sooner if
    // SignatureHash shows their behavior actually changed), not on every Home
    // page load. DB-backed rather than an in-memory singleton (the usual
    // pattern here, see PublicStatsService) so it survives an app restart and
    // isn't lost across multiple API instances.
    public class UserRecommendationCache : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Serialized DreamTripRecommendResponse.
        public string ResponseJson { get; set; } = string.Empty;

        // Cheap fingerprint of the behavior signals (searched/booked/journaled
        // country names) that produced ResponseJson - if this no longer matches
        // the user's CURRENT signals, the cache is stale even if under 24h old.
        public string SignatureHash { get; set; } = string.Empty;

        public DateTime GeneratedAt { get; set; }
    }
}
