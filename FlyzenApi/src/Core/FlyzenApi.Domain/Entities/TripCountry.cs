using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TripCountry : BaseEntity
    {
        public string NameAz { get; set; } = string.Empty;
        // Nullable: admin now only types NameAz; NameEn/NameRu are filled by the
        // OpenRouter auto-translate call on save and null means "translation
        // pending/failed" until an admin retries it (see ContentTranslationService).
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string FlagCode { get; set; } = string.Empty;
        public string? CoverImage { get; set; }

        public ICollection<TripCity> Cities { get; set; } = new List<TripCity>();
    }
}
