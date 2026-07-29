using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripCountry : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string FlagCode { get; set; } = string.Empty;
        public string? CoverImage { get; set; }

        public ICollection<TripCity> Cities { get; set; } = new List<TripCity>();
    }
}
