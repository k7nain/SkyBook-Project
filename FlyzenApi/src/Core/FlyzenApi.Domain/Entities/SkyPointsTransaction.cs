using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class SkyPointsTransaction : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Always the positive magnitude actually applied to User.SkyPointsBalance
        // (never negative) - Type carries the direction. On a Reversed row this
        // is the amount actually removed, which may be less than the original
        // Earned amount if the balance had already partly been redeemed
        // elsewhere (clamped at 0, never driven negative).
        public int Amount { get; set; }
        public SkyPointsTransactionType Type { get; set; }

        public Guid? RelatedBookingId { get; set; }
        public Booking? RelatedBooking { get; set; }
    }
}
