using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace FlyzenApi.Application.Implementations.Services
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailService emailService,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId) =>
            (await _notificationRepository.GetByUserIdAsync(userId)).Select(n => n.ToDto());

        public async Task MarkAsReadAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification is null || notification.UserId != userId)
                throw new NotFoundException("Notification not found.");

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);
        }

        public async Task DeleteAsync(Guid id, Guid userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification is null || notification.UserId != userId)
                throw new NotFoundException("Notification not found.");

            await _notificationRepository.DeleteAsync(notification);
        }

        public async Task DeleteAllAsync(Guid userId) =>
            await _notificationRepository.DeleteAllByUserIdAsync(userId);

        public async Task CreateAsync(
            Guid userId,
            string recipientEmail,
            string title,
            string message,
            NotificationType type,
            Guid? bookingId = null,
            string? emailSubject = null,
            string? emailHtmlBody = null)
        {
            await _notificationRepository.AddAsync(new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                BookingId = bookingId,
            });

            if (emailSubject is null || emailHtmlBody is null)
                return;

            try
            {
                await _emailService.SendAsync(recipientEmail, emailSubject, emailHtmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send {Type} notification email to {Email}.", type, recipientEmail);
            }
        }
    }
}
