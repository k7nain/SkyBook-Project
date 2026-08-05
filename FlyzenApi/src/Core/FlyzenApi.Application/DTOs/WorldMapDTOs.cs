using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class MapMarkerDto
    {
        public Guid Id { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string CountryCode { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Only populated for GetUserRecommendedMarkersAsync results - null everywhere
        // else. F1's Dream Trip AI is a stateless call with no persisted score, so
        // there is nothing to source this from yet (see IWorldMapService).
        public double? MatchScore { get; set; }

        public string? ShortDescription { get; set; }
    }

    public class MapBoundsRequest
    {
        [Range(-90, 90)]
        public double NorthLat { get; set; }

        [Range(-90, 90)]
        public double SouthLat { get; set; }

        // No [Range] here (unlike the lat fields): a real Leaflet viewport at
        // low zoom with worldCopyJump can legitimately report bounds spanning
        // more than 360 degrees, or offset by several full world-widths after
        // repeated panning (observed up to +/-945 in practice) - these are
        // valid queries, not bad input. WorldMapService.NormalizeLongitudeBounds
        // wraps them into a single [-180, 180) window before querying.
        public double EastLng { get; set; }

        public double WestLng { get; set; }

        [Range(1, 20)]
        public int ZoomLevel { get; set; } = 2;

        // "az" | "en" | "ru", same three locales as the rest of the catalog - falls
        // back to "az" like DreamTripAiService does when missing/unrecognized.
        public string? Lang { get; set; }
    }

    public class DestinationDetailDto
    {
        public Guid Id { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string CountryCode { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string? CityDescription { get; set; }
        public string? ThumbnailUrl { get; set; }
        public List<string> GalleryImageUrls { get; set; } = new();
        public double? MatchScore { get; set; }

        // Best-effort name match against the bookable City used by flight search -
        // TripCity (Dream Trip's world-browsing model) and City (the booking
        // model) are separate entities with no FK, so this is null whenever
        // there's no bookable city by this destination's city name (see
        // WorldMapService.GetDestinationDetailAsync). Null means the World Map
        // popup's "search flights" link has nothing to link to yet, not an error.
        public Guid? BookableCityId { get; set; }
    }
}
