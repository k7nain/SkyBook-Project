using System.Globalization;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Pricing
{
    // Mirrors FlightNotificationBackgroundService's shape (plain BackgroundService
    // loop, no Quartz/Hangfire - see that class' own comment on why). This job
    // only ever PROPOSES a price change (see PriceProposal) - by explicit product
    // decision, it never writes Flight.BasePrice itself. An admin must approve
    // via PriceProposalService.ApproveAsync, which reuses the existing
    // AdminService.UpdateFlightPriceAsync path (same one manual admin price
    // edits already take), so the existing user-facing PriceChange notification
    // keeps working unchanged - this job only ever adds a NEW admin-facing
    // notification type (PriceProposalCreated), never touches the user-facing one.
    public class DynamicPricingBackgroundService : BackgroundService
    {
        // Demand score weights - occupancy matters most (a nearly-full flight is
        // the clearest signal), booking velocity next (is demand accelerating
        // right now), then how soon it departs (urgency alone, with low
        // occupancy and no recent bookings, shouldn't swing the price much).
        private const decimal OccupancyWeight = 0.5m;
        private const decimal VelocityWeight = 0.3m;
        private const decimal UrgencyWeight = 0.2m;

        // Urgency ramps up linearly starting this many days before departure -
        // beyond this window, how-soon-is-it contributes nothing to the score.
        private const double UrgencyWindowDays = 14;

        // Booking velocity compares bookings placed in the most recent window
        // against the one immediately before it - a simple, explainable
        // "is demand accelerating" signal rather than a black-box trend model.
        private static readonly TimeSpan VelocityWindow = TimeSpan.FromHours(48);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DynamicPricingOptions _options;
        private readonly ILogger<DynamicPricingBackgroundService> _logger;

        public DynamicPricingBackgroundService(
            IServiceScopeFactory scopeFactory,
            IOptions<DynamicPricingOptions> options,
            ILogger<DynamicPricingBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _options = options.Value;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CheckIntervalMinutes));

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EvaluateFlightsAsync(stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Dynamic pricing background job failed.");
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

        private async Task EvaluateFlightsAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var flightRepository = scope.ServiceProvider.GetRequiredService<IFlightRepository>();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var proposalRepository = scope.ServiceProvider.GetRequiredService<IPriceProposalRepository>();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            var flights = (await flightRepository.GetActiveForPricingCheckAsync()).ToList();
            if (flights.Count == 0)
                return;

            var admins = (await userRepository.GetAdminsAsync()).ToList();
            var nowUtc = DateTime.UtcNow;

            foreach (var flight in flights)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Don't stack a second proposal on top of one an admin hasn't
                // decided on yet - wait for Approve/Reject first.
                if (await proposalRepository.HasPendingForFlightAsync(flight.Id))
                    continue;

                if (flight.Seats.Count == 0)
                    continue;

                var bookingTimestamps = await bookingRepository.GetBookingCreationTimestampsByFlightIdAsync(flight.Id);
                var proposal = ComputeProposal(flight, bookingTimestamps, nowUtc, _options.MaxPriceChangePercent, _options.MinChangePercentToPropose);
                if (proposal is null)
                    continue;

                await proposalRepository.AddAsync(proposal);

                if (admins.Count == 0)
                    continue;

                var route = $"{flight.DepartureCity.Name} -> {flight.ArrivalCity.Name}";
                var args = new Dictionary<string, string>
                {
                    ["route"] = route,
                    ["currentPrice"] = FormatPrice(proposal.CurrentPrice, flight.Currency),
                    ["suggestedPrice"] = FormatPrice(proposal.SuggestedPrice, flight.Currency),
                };

                foreach (var admin in admins)
                {
                    await notificationService.CreateAsync(
                        admin,
                        "notifications.priceProposalCreated.title",
                        "notifications.priceProposalCreated.body",
                        args,
                        NotificationType.PriceProposalCreated,
                        bookingId: null);
                }
            }
        }

        private static string FormatPrice(decimal price, string currency) =>
            $"{price.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";

        // Returns null when either there's nothing meaningful to propose (the
        // computed change is under MinChangePercentToPropose) or the flight has
        // no seats to reason about at all.
        internal static PriceProposal? ComputeProposal(Flight flight, IReadOnlyList<DateTime> bookingTimestamps, DateTime nowUtc, decimal maxChangePercent, decimal minChangePercentToPropose = 2m)
        {
            var totalSeats = flight.Seats.Count;
            if (totalSeats == 0)
                return null;

            var bookedSeats = flight.Seats.Count(s => !s.IsAvailable);
            var occupancyRate = (decimal)bookedSeats / totalSeats;

            var recentBookings = bookingTimestamps.Count(t => t >= nowUtc - VelocityWindow && t < nowUtc);
            var priorBookings = bookingTimestamps.Count(t => t >= nowUtc - VelocityWindow - VelocityWindow && t < nowUtc - VelocityWindow);
            // No prior-window data to compare against: treat "still nothing
            // happening" as flat demand, and "bookings appeared from nothing" as
            // strong (capped) acceleration, rather than an undefined 0/0 ratio.
            double velocityRatio = priorBookings == 0
                ? (recentBookings == 0 ? 1.0 : 2.0)
                : (double)recentBookings / priorBookings;
            var velocityNormalized = (decimal)Math.Clamp(velocityRatio / 2.0, 0.0, 1.0);

            var daysToDeparture = (flight.DepartureTime - nowUtc).TotalDays;
            var urgencyFactor = (decimal)Math.Clamp(1.0 - daysToDeparture / UrgencyWindowDays, 0.0, 1.0);

            var demandScore = occupancyRate * OccupancyWeight + velocityNormalized * VelocityWeight + urgencyFactor * UrgencyWeight;

            // demandScore's midpoint (0.5 - "average" occupancy/velocity/urgency)
            // is defined as 0% change; it scales linearly out to ±maxChangePercent
            // at the extremes (demandScore 0 or 1), so the cap is reached exactly
            // at the edges of the score's own range, not before or after.
            var rawPercentChange = (demandScore - 0.5m) * 2m * maxChangePercent;
            var percentChange = Math.Clamp(rawPercentChange, -maxChangePercent, maxChangePercent);

            if (Math.Abs(percentChange) < minChangePercentToPropose)
                return null;

            var currentPrice = flight.BasePrice;
            var suggestedPrice = Math.Round(currentPrice * (1 + percentChange / 100m), 2);
            if (suggestedPrice == currentPrice)
                return null;

            return new PriceProposal
            {
                FlightId = flight.Id,
                CurrentPrice = currentPrice,
                SuggestedPrice = suggestedPrice,
                OccupancyRate = occupancyRate,
                VelocityFactor = velocityNormalized,
                UrgencyFactor = urgencyFactor,
                DemandScore = demandScore,
            };
        }
    }
}
