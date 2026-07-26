using FlyzenApi.Application.DTOs;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId);
        Task MarkAsReadAsync(Guid id, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task DeleteAllAsync(Guid userId);

        // Called from other services (booking creation, admin flight-price updates,
        // the trip-reminder background job) - not exposed via any controller directly.
        // The caller passes the recipient's email directly (it already has the User
        // loaded in every call site) rather than have this service re-fetch it.
        Task CreateAsync(Guid userId, string recipientEmail, string title, string message, NotificationType type, Guid? bookingId = null, string? emailSubject = null, string? emailHtmlBody = null);
    }
}
