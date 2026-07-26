using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripCity : BaseEntity
    {
        public Guid CountryId { get; set; }
        public TripCountry Country { get; set; } = null!;

        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? ShortDescription { get; set; }

        public ICollection<TripPlace> Places { get; set; } = new List<TripPlace>();
    }
}
