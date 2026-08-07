using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class TravelJournalImage : BaseEntity
    {
        public Guid JournalId { get; set; }
        public TravelJournal Journal { get; set; } = null!;

        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }
    }
}
