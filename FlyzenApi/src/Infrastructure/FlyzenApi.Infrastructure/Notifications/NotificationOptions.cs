namespace FlyzenApi.Infrastructure.Notifications
{
    public class NotificationOptions
    {
        public const string SectionName = "Notifications";

        public int[] TripReminderDaysBefore { get; set; } = new[] { 3, 1 };
    }
}
