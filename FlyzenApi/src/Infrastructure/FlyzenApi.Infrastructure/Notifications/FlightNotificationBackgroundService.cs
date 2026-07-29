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
    // Mirrors TripReminderBackgroundService's shape (plain BackgroundService loop,
    // no Quartz/Hangfire) but runs far more often, since "departs in less than
    // 1 hour" needs sub-hourly precision rather than a once-a-day check.
    public class FlightNotificationBackgroundService : BackgroundService
    {
        private static readonly TimeSpan DepartureWindow24h = TimeSpan.FromHours(24);
        private static readonly TimeSpan DepartureWindow1h = TimeSpan.FromHours(1);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly NotificationOptions _options;
        private readonly ILogger<FlightNotificationBackgroundService> _logger;

        public FlightNotificationBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<NotificationOptions> options,
            ILogger<FlightNotificationBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.FlightNotificationCheckIntervalMinutes));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckFlightsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Flight notification background job failed.");
                }

                try
                {
                    await Task.Delay(interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }

        private async Task CheckFlightsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var flightRepository = scope.ServiceProvider.GetRequiredService<IFlightRepository>();
            var logRepository = scope.ServiceProvider.GetRequiredService<IFlightNotificationLogRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var flights = (await flightRepository.GetActiveForNotificationCheckAsync()).ToList();
            if (flights.Count == 0)
                return;

            var admins = (await userRepository.GetAdminsAsync()).ToList();
            if (admins.Count == 0)
                return;

            // Flight.DepartureTime/ArrivalTime are stored as UTC (see AdminService.
            // CreateFlightAsync), so comparing against DateTime.UtcNow directly is
            // correct without any timezone conversion.
            var nowUtc = DateTime.UtcNow;

            foreach (var flight in flights)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await ProcessDepartureAsync(flight, nowUtc, logRepository, notificationService, flightRepository, admins);
                await ProcessArrivalAsync(flight, nowUtc, logRepository, notificationService, flightRepository, admins);
            }
        }

        private static async Task ProcessDepartureAsync(
            Flight flight,
            DateTime nowUtc,
            IFlightNotificationLogRepository logRepository,
            INotificationService notificationService,
            IFlightRepository flightRepository,
            List<User> admins)
        {
            var route = $"{flight.FlightNumber} to {flight.ArrivalCity.Name}";
            var args = new Dictionary<string, string> { ["route"] = route };

            if (nowUtc >= flight.DepartureTime)
            {
                if (await logRepository.ExistsAsync(flight.Id, FlightNotificationEvent.Departed))
                    return;

                await NotifyAdminsAsync(notificationService, admins, "notifications.flightDeparted.title", "notifications.flightDeparted.body", args, NotificationType.FlightDeparted);
                await logRepository.AddAsync(new FlightNotificationLog { FlightId = flight.Id, EventType = FlightNotificationEvent.Departed });

                flight.Status = FlightStatus.Departed;
                await flightRepository.UpdateAsync(flight);
                return;
            }

            var timeToDeparture = flight.DepartureTime - nowUtc;

            if (timeToDeparture <= DepartureWindow1h)
            {
                if (await logRepository.ExistsAsync(flight.Id, FlightNotificationEvent.DepartureWithin1Hour))
                    return;

                await NotifyAdminsAsync(notificationService, admins, "notifications.flightDepartureReminder.title", "notifications.flightDepartureReminder.body1h", args, NotificationType.FlightDepartureReminder);
                await logRepository.AddAsync(new FlightNotificationLog { FlightId = flight.Id, EventType = FlightNotificationEvent.DepartureWithin1Hour });
            }
            else if (timeToDeparture <= DepartureWindow24h)
            {
                if (await logRepository.ExistsAsync(flight.Id, FlightNotificationEvent.DepartureWithin24Hours))
                    return;

                await NotifyAdminsAsync(notificationService, admins, "notifications.flightDepartureReminder.title", "notifications.flightDepartureReminder.body24h", args, NotificationType.FlightDepartureReminder);
                await logRepository.AddAsync(new FlightNotificationLog { FlightId = flight.Id, EventType = FlightNotificationEvent.DepartureWithin24Hours });
            }
        }

        private static async Task ProcessArrivalAsync(
            Flight flight,
            DateTime nowUtc,
            IFlightNotificationLogRepository logRepository,
            INotificationService notificationService,
            IFlightRepository flightRepository,
            List<User> admins)
        {
            if (nowUtc < flight.ArrivalTime)
                return;

            if (await logRepository.ExistsAsync(flight.Id, FlightNotificationEvent.Arrived))
                return;

            var route = $"{flight.FlightNumber} to {flight.ArrivalCity.Name}";
            var args = new Dictionary<string, string> { ["route"] = route };
            await NotifyAdminsAsync(notificationService, admins, "notifications.flightArrived.title", "notifications.flightArrived.body", args, NotificationType.FlightArrived);
            await logRepository.AddAsync(new FlightNotificationLog { FlightId = flight.Id, EventType = FlightNotificationEvent.Arrived });

            flight.Status = FlightStatus.Completed;
            await flightRepository.UpdateAsync(flight);
        }

        private static async Task NotifyAdminsAsync(
            INotificationService notificationService,
            List<User> admins,
            string titleKey,
            string bodyKey,
            IReadOnlyDictionary<string, string> args,
            NotificationType type)
        {
            foreach (var admin in admins)
            {
                // No emailSubject/emailHtmlBody - these are in-app-only admin alerts.
                // Each admin's own LanguagePreference is used (not the language of
                // whoever triggered the underlying event, since there isn't one here
                // anyway - this is a background job, not an admin action).
                await notificationService.CreateAsync(admin, titleKey, bodyKey, args, type);
            }
        }
    }
}
