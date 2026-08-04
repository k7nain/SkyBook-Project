using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.SignalR;

namespace FlyzenApi.API.Hubs
{
    public class SignalRNotificationPusher : INotificationPusher
    {
        private const string ReceiveEvent = "ReceiveNotification";

        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationPusher(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        // A no-op (not an error) if the user has no live connection - IHubContext
        // simply drops the message, which is exactly the desired "additional
        // channel on top of the stored row" behavior.
        public Task PushAsync(Guid userId, NotificationDto notification, CancellationToken cancellationToken = default) =>
            _hubContext.Clients.User(userId.ToString()).SendAsync(ReceiveEvent, notification, cancellationToken);
    }
}
