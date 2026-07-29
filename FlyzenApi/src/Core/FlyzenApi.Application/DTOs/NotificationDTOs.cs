using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? TitleKey { get; set; }
        public string? BodyKey { get; set; }
        public Dictionary<string, string>? Params { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public Guid? BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
