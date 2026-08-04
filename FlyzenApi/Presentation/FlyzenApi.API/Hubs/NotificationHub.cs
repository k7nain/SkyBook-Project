using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace FlyzenApi.API.Hubs
{
    // Server-push only - clients never call a method on this hub, they just
    // connect and listen for the "ReceiveNotification" event sent via
    // SignalRNotificationPusher. Relies on SignalR's default IUserIdProvider,
    // which reads ClaimTypes.NameIdentifier - the same claim the JWT already
    // carries and every controller already reads via User.GetUserId() - so
    // Clients.User(userId) works with no custom provider needed.
    [Authorize]
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override Task OnConnectedAsync()
        {
            _logger.LogInformation("Notification hub connection {ConnectionId} established for user {UserId}.",
                Context.ConnectionId, Context.UserIdentifier);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Notification hub connection {ConnectionId} closed for user {UserId}.",
                Context.ConnectionId, Context.UserIdentifier);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
