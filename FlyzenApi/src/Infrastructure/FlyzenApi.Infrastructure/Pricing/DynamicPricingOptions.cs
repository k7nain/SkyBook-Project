namespace FlyzenApi.Infrastructure.Pricing
{
    public class DynamicPricingOptions
    {
        public const string SectionName = "DynamicPricing";

        // Global safety cap (task requirement: "max ±15% from the base price
        // admin originally set"). Applies to every flight - no per-flight
        // override exists yet, this is the "globally" half of the "configurable
        // per flight or globally" option the feature plan offered.
        public decimal MaxPriceChangePercent { get; set; } = 15m;

        // How often the job re-evaluates every active flight. Pricing doesn't
        // need FlightNotificationBackgroundService's sub-hourly precision -
        // demand shifts over hours/days, not minutes.
        public int CheckIntervalMinutes { get; set; } = 60;

        // A proposal below this % change (in either direction) isn't worth an
        // admin's attention - skip creating it rather than spamming the
        // approval queue with negligible adjustments.
        public decimal MinChangePercentToPropose { get; set; } = 2m;
    }
}
