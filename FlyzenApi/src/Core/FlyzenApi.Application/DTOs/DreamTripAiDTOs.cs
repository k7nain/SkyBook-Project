using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class DreamTripRecommendRequest
    {
        // "low" | "medium" | "high" - kept as a free label rather than an enum since
        // it only ever flows into the AI prompt as text, never compared/branched on.
        [Required, MaxLength(20)]
        public string BudgetLevel { get; set; } = string.Empty;

        // e.g. "now", "summer", "winter", or a specific month name - same reasoning as BudgetLevel.
        [Required, MaxLength(30)]
        public string TravelMonth { get; set; } = string.Empty;

        [Required, MinLength(1), MaxLength(7)]
        public List<string> Interests { get; set; } = new();
    }

    public class DreamTripRecommendationDto
    {
        public TripCountryDto Country { get; set; } = null!;

        // AI-generated explanation of why this country fits the given answers.
        public string Reason { get; set; } = string.Empty;
    }

    public class DreamTripRecommendResponse
    {
        public List<DreamTripRecommendationDto> Recommendations { get; set; } = new();

        // Set when nothing matched well (or the AI wants to add a caveat) - the
        // frontend should always be able to show something meaningful even when
        // Recommendations is empty.
        public string? Message { get; set; }
    }
}
