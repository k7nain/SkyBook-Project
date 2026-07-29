using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class City : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AirportCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Climate { get; set; }
        public string? Attractions { get; set; }

        // IANA time zone name (e.g. "Asia/Baku") for this airport's city -
        // flight departure/arrival times are stored in UTC and converted to
        // the relevant city's zone only at display time (see api/mappers.js).
        public string TimeZoneId { get; set; } = "UTC";

        public ICollection<CityGalleryImage> GalleryImages { get; set; } = new List<CityGalleryImage>();
        public ICollection<Flight> FlightsFromHere { get; set; } = new List<Flight>();
        public ICollection<Flight> FlightsToHere { get; set; } = new List<Flight>();
    }
}