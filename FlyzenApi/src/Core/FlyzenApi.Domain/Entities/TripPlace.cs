using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripPlace : BaseEntity
    {
        public Guid CityId { get; set; }
        public TripCity City { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<TripPlaceImage> Images { get; set; } = new List<TripPlaceImage>();
    }
}
