using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TravelJournalImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class TravelJournalDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserAvatarUrl { get; set; }
        // Always true - every row in this table required a completed-trip
        // booking to create (see TravelJournalService.CreateAsync). Exposed as
        // a field anyway so the client doesn't need to hardcode the badge.
        public bool IsVerifiedTraveler { get; set; } = true;
        public Guid BookingId { get; set; }
        public Guid DestinationCityId { get; set; }
        public string DestinationCityName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public int? Rating { get; set; }
        public List<TravelJournalImageDto> Images { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTravelJournalRequest
    {
        [Required]
        public Guid BookingId { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(3000)]
        public string Body { get; set; } = string.Empty;

        [Range(1, 5)]
        public int? Rating { get; set; }

        // Each URL must already exist (uploaded beforehand via
        // POST /api/travel-journals/upload-image), same "upload first, attach
        // by URL after" flow as AddTripPlaceImageRequest.
        public List<string> ImageUrls { get; set; } = new();
    }
}
