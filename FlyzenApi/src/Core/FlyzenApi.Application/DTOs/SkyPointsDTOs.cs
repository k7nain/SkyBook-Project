using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class SkyPointsTransactionDto
    {
        public Guid Id { get; set; }
        public int Amount { get; set; }
        public SkyPointsTransactionType Type { get; set; }
        // Null when not tied to a booking (shouldn't normally happen today -
        // every Earned/Redeemed/Reversed row currently has one - but kept
        // nullable to match the entity's own optional FK).
        public string? RelatedBookingPnr { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
