using FlyzenApi.Application.Common;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Notifications
{
    // No scheduling library (Quartz/Hangfire) exists in this project and none is
    // warranted for a once-a-day check - a plain BackgroundService loop is the
    // natural fit. Runs once at startup, then every 24h.
    public class TripReminderBackgroundService : BackgroundService
    {
        private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationOptions _options;
        private readonly ILogger<TripReminderBackgroundService> _logger;

        public TripReminderBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<NotificationOptions> options,
            ILogger<TripReminderBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendDueRemindersAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Trip reminder background job failed.");
                }

                try
                {
                    await Task.Delay(RunInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task SendDueRemindersAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var bookingReminderRepository = scope.ServiceProvider.GetRequiredService<IBookingReminderRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            foreach (var daysBefore in _options.TripReminderDaysBefore)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var targetDate = DateTime.UtcNow.Date.AddDays(daysBefore);
                var bookings = await bookingRepository.GetBookingsDepartingOnAsync(targetDate);

                foreach (var booking in bookings)
                {
                    if (await bookingReminderRepository.ExistsAsync(booking.Id, daysBefore))
                        continue;

                    var route = $"{booking.Flight.DepartureCity.Name} -> {booking.Flight.ArrivalCity.Name}";
                    await notificationService.CreateAsync(
                        booking.UserId,
                        booking.User.Email,
                        "Your trip is coming up",
                        $"Your flight {route} departs in {daysBefore} day{(daysBefore == 1 ? "" : "s")}.",
                        NotificationType.TripReminder,
                        bookingId: booking.Id,
                        emailSubject: "Your trip is coming up",
                        emailHtmlBody: EmailTemplates.BuildTripReminderEmail(
                            booking.User.FirstName, route, booking.Flight.DepartureTime.ToString("f"), daysBefore));

                    await bookingReminderRepository.AddAsync(new BookingReminder
                    {
                        BookingId = booking.Id,
                        DaysBefore = daysBefore,
                    });
                }
            }
        }
    }
}
