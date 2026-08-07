using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class PriceProposalDto
    {
        public Guid Id { get; set; }
        public Guid FlightId { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Currency { get; set; } = "AZN";
        public decimal CurrentPrice { get; set; }
        public decimal SuggestedPrice { get; set; }

        // Formula breakdown - see DynamicPricingBackgroundService.ComputeProposal.
        public decimal OccupancyRate { get; set; }
        public decimal VelocityFactor { get; set; }
        public decimal UrgencyFactor { get; set; }
        public decimal DemandScore { get; set; }

        public PriceProposalStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? DecidedAt { get; set; }
        public string? DecidedByName { get; set; }
    }
}
