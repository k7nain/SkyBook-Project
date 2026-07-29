using System;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Point-in-time rendered snapshot, in the recipient's language at the
        // moment this notification was created - a fallback for any client
        // that doesn't (yet) re-render from TitleKey/BodyKey/ParamsJson.
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        // Translation key + serialized args (Dictionary<string,string> as JSON)
        // so the frontend can dynamically re-render this notification in
        // whatever language the user currently has selected, even if they
        // changed it after this row was created. Null on rows created before
        // this field existed - those fall back to Title/Message above.
        public string? TitleKey { get; set; }
        public string? BodyKey { get; set; }
        public string? ParamsJson { get; set; }

        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;

        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
}
