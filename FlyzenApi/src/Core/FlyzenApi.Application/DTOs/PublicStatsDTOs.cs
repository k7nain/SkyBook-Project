namespace FlyzenApi.Application.DTOs
{
    // Public marketing-page numbers only (About Us) - deliberately just three
    // plain counts, never anything user-identifiable. See PublicStatsService
    // for exactly what each count means and why.
    public class PublicStatsDto
    {
        public int Destinations { get; set; }
        public int HappyCustomers { get; set; }
        public int CompletedFlights { get; set; }
    }
}
