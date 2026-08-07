using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    // A demand-based price change DynamicPricingBackgroundService has computed
    // and proposed, but NOT applied - by explicit product decision (see the
    // feature plan), a bad formula run must never be able to silently 10x a
    // price. The price only actually changes once an admin approves (see
    // PriceProposalService.ApproveAsync, which routes through the existing
    // AdminService.UpdateFlightPriceAsync - the same path a manual admin price
    // edit already takes, so the existing user-facing PriceChange notification
    // fires exactly as it always has, with no new "auto-write" logic).
    public class PriceProposal : BaseEntity
    {
        public Guid FlightId { get; set; }
        public Flight Flight { get; set; } = null!;

        // Snapshot of Flight.BasePrice at proposal time - Flight.BasePrice may
        // have moved on (another approved proposal, a manual admin edit) by the
        // time this is reviewed, so the proposal keeps its own record of what it
        // was actually comparing against.
        public decimal CurrentPrice { get; set; }
        public decimal SuggestedPrice { get; set; }

        // Formula breakdown (see DynamicPricingBackgroundService.ComputeProposal) -
        // kept for admin-facing transparency ("why is this being suggested"),
        // not just the two price numbers.
        public decimal OccupancyRate { get; set; }
        public decimal VelocityFactor { get; set; }
        public decimal UrgencyFactor { get; set; }
        public decimal DemandScore { get; set; }

        public PriceProposalStatus Status { get; set; } = PriceProposalStatus.Pending;

        public DateTime? DecidedAt { get; set; }
        public Guid? DecidedByUserId { get; set; }
        public User? DecidedByUser { get; set; }
    }
}
