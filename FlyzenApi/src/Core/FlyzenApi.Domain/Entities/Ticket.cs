using System;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class Ticket : BaseEntity
    {
        public Guid BookingId { get; set; }
        public Booking Booking { get; set; } = null!;
        public string TicketNumber { get; set; } = string.Empty;
        public string QrCodeData { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
    }
}