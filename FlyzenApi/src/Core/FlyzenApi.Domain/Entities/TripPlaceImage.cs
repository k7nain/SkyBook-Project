using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripPlaceImage : BaseEntity
    {
        public Guid PlaceId { get; set; }
        public TripPlace Place { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;
    }
}
