using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine() =>
            Ok(await _notificationService.GetMineAsync(User.GetUserId()));

        [HttpPost("{id:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid id)
        {
            await _notificationService.MarkAsReadAsync(id, User.GetUserId());
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _notificationService.MarkAllAsReadAsync(User.GetUserId());
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _notificationService.DeleteAsync(id, User.GetUserId());
            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAll()
        {
            await _notificationService.DeleteAllAsync(User.GetUserId());
            return NoContent();
        }
    }
}
