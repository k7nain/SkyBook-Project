using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public Guid? BookingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
