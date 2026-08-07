using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    // One row per logged-in user's flight search (origin/destination pair) -
    // the "past search queries" signal for the "Sizin uчun" personalized
    // recommendations (see DreamTripAiService.RecommendForYouAsync). Only
    // logged for authenticated searches - FlightsController.Search itself
    // stays open to guests, but an anonymous search has no user to
    // personalize for, so there's nothing useful to record.
    public class SearchLog : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Null in "any origin" search mode (e.g. a World Map marker click) -
        // see FlightRepository.SearchAsync.
        public Guid? FromCityId { get; set; }
        public City? FromCity { get; set; }

        public Guid ToCityId { get; set; }
        public City ToCity { get; set; } = null!;
    }
}
