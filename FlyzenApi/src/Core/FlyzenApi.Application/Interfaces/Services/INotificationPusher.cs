using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    // Additional real-time delivery channel on top of the stored Notification row
    // (see NotificationService.CreateAsync) - a no-op if the user has no live
    // SignalR connection, never a replacement for the stored notification itself.
    // Implemented in the Presentation layer (FlyzenApi.API.Hubs), since that's
    // where the Hub/IHubContext live.
    public interface INotificationPusher
    {
        Task PushAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default);
    }
}
