using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripCity : BaseEntity
    {
        public Guid CountryId { get; set; }
        public TripCountry Country { get; set; } = null!;

        public string NameAz { get; set; } = string.Empty;
        // Nullable: see TripCountry.NameEn for why (AZ-only admin input + async translate).
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string? Image { get; set; }
        public string? ShortDescriptionAz { get; set; }
        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionRu { get; set; }

        public ICollection<TripPlace> Places { get; set; } = new List<TripPlace>();
    }
}
