using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class Booking : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid FlightId { get; set; }
        public Flight Flight { get; set; } = null!;
        
        public string PNR { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Pending;
        
        public ICollection<BookingPassenger> Passengers { get; set; } = new List<BookingPassenger>();
        public Ticket? Ticket { get; set; }
    }
}