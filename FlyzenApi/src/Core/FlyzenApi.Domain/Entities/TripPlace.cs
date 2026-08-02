using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripPlace : BaseEntity
    {
        public Guid CityId { get; set; }
        public TripCity City { get; set; } = null!;

        public string NameAz { get; set; } = string.Empty;
        // Nullable: see TripCountry.NameEn for why (AZ-only admin input + async translate).
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string? DescriptionAz { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionRu { get; set; }
        // Fixed, language-agnostic key (e.g. "Historical", "Museum") - the display
        // label is resolved client-side via i18n (dreamTrip.categories.*), the same
        // way notification types map to translated labels, so it stays valid for
        // both the icon lookup and any language.
        public string? Category { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public ICollection<TripPlaceImage> Images { get; set; } = new List<TripPlaceImage>();
    }
}
