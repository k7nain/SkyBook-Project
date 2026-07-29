namespace FlyzenApi.Infrastructure.Notifications
{
    public class NotificationOptions
    {
        public const string SectionName = "Notifications";

        public int[] TripReminderDaysBefore { get; set; } = new[] { 3, 1 };

        // How often FlightNotificationBackgroundService checks flight departure/
        // arrival times. Kept short (unlike TripReminderDaysBefore's daily cadence)
        // since "departs in less than 1 hour" needs sub-hourly precision.
        public int FlightNotificationCheckIntervalMinutes { get; set; } = 5;
    }
}
