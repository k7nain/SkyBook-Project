using System.Text.Json;
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
        private static readonly HashSet<NotificationType> RealTimePushTypes = new()
        {
            NotificationType.PriceChange,
            NotificationType.CheckInOpen,
            NotificationType.GateChanged,
            NotificationType.FlightDelayed,
            NotificationType.BoardingReminder,
            NotificationType.CheckInCompleted,
            NotificationType.FlightCancelled,
        };

        private readonly INotificationRepository _notificationRepository;
        private readonly IEmailService _emailService;
        private readonly ITranslationService _translationService;
        private readonly INotificationPusher _notificationPusher;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            INotificationRepository notificationRepository,
            IEmailService emailService,
            ITranslationService translationService,
            INotificationPusher notificationPusher,
            ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _emailService = emailService;
            _translationService = translationService;
            _notificationPusher = notificationPusher;
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

        public async Task MarkAllAsReadAsync(Guid userId) =>
            await _notificationRepository.MarkAllAsReadByUserIdAsync(userId);

        public async Task CreateAsync(
            User recipient,
            string titleKey,
            string bodyKey,
            IReadOnlyDictionary<string, string>? args,
            NotificationType type,
            Guid? bookingId = null,
            string? emailSubject = null,
            string? emailHtmlBody = null)
        {
            var language = recipient.LanguagePreference;

            var notification = new Notification
            {
                UserId = recipient.Id,
                Title = _translationService.Translate(language, titleKey, args),
                Message = _translationService.Translate(language, bodyKey, args),
                TitleKey = titleKey,
                BodyKey = bodyKey,
                ParamsJson = args is null ? null : JsonSerializer.Serialize(args),
                Type = type,
                BookingId = bookingId,
            };
            await _notificationRepository.AddAsync(notification);

            // Real-time push: time-sensitive, flight-change-triggered types only
            // (price updates, plus the newer check-in/gate/delay/boarding set -
            // see AdminService.UpdateFlightPriceAsync/UpdateFlightOperationalStatusAsync
            // and FlightNotificationBackgroundService). Booking confirmations and
            // trip reminders aren't time-critical the same way, so they stay
            // poll/refresh-based.
            if (RealTimePushTypes.Contains(type))
            {
                try
                {
                    await _notificationPusher.PushAsync(recipient.Id, notification.ToDto());
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to push real-time notification to user {UserId}.", recipient.Id);
                }
            }

            if (emailSubject is null || emailHtmlBody is null)
                return;

            try
            {
                await _emailService.SendAsync(recipient.Email, emailSubject, emailHtmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send {Type} notification email to {Email}.", type, recipient.Email);
            }
        }
    }
}
