using System;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    // Dedupe record: one row per (flight, event type) pair, so the background
    // job can tell "was this event already announced to admins" with a plain
    // existence check, mirroring BookingReminder for trip reminders.
    public class FlightNotificationLog : BaseEntity
    {
        public Guid FlightId { get; set; }
        public Flight Flight { get; set; } = null!;

        public FlightNotificationEvent EventType { get; set; }
    }
}
