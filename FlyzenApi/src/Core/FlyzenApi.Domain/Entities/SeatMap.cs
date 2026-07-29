using System;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class SeatMap : BaseEntity
    {
        public Guid FlightId { get; set; }
        public Flight Flight { get; set; } = null!;
        public string SeatNumber { get; set; } = string.Empty;
        public SeatClass Class { get; set; }
        public bool IsAvailable { get; set; } = true;
        public decimal PriceMultiplier { get; set; } = 1.0m;

        public Guid? BookingId { get; set; }
        public Booking? Booking { get; set; }
    }
}