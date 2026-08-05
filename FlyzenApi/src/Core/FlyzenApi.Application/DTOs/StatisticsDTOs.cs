namespace FlyzenApi.Application.DTOs
{
    // GET /api/admin/statistics response - one call for everything an admin
    // dashboard's stat cards/charts need, instead of the client deriving
    // totals from GET /api/admin/bookings + /users + /flights client-side.
    public class AdminStatisticsDto
    {
        // Sum of TotalPrice across all non-cancelled bookings, in AZN (the
        // app's single stored-price currency - see AdminService.CreateFlightAsync).
        // Mirrors RevenueByCurrency["AZN"] today; kept as its own field so the
        // dashboard has one obvious top-line number without summing the dictionary.
        public decimal TotalRevenue { get; set; }
        public Dictionary<string, decimal> RevenueByCurrency { get; set; } = new();

        public int TotalBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CancelledBookings { get; set; }

        // Verified (IsEmailConfirmed) accounts only - matches the default
        // GET /api/admin/users filter, so this number matches what an admin
        // sees on the Users tab with no status filter applied.
        public int TotalUsers { get; set; }
        public int TotalFlights { get; set; }

        // Platform-wide Sky Points totals - issued (Earned, before any
        // Reversed clawbacks) vs. actually spent as checkout discounts.
        public long TotalSkyPointsIssued { get; set; }
        public long TotalSkyPointsRedeemed { get; set; }

        public List<RevenueTrendPointDto> RevenueTrend { get; set; } = new();
    }

    public class RevenueTrendPointDto
    {
        // ISO "yyyy-MM", e.g. "2026-07" - sorts and parses trivially on any client.
        public string Period { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }
}
