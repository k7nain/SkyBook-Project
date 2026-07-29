using System;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    // Dedupe record: one row per (booking, reminder window) pair, so the daily
    // background job can tell "was the 3-day reminder already sent for this
    // booking" with a plain existence check instead of string-matching notifications.
    public class BookingReminder : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;

        public int DaysBefore { get; set; }
    }
}
