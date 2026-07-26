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

        public ICollection<CityGalleryImage> GalleryImages { get; set; } = new List<CityGalleryImage>();
        public ICollection<Flight> FlightsFromHere { get; set; } = new List<Flight>();
        public ICollection<Flight> FlightsToHere { get; set; } = new List<Flight>();
    }
}