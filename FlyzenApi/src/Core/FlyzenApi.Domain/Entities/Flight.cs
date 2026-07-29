using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class Flight : BaseEntity
    {
        public string FlightNumber { get; set; } = string.Empty;

        public Guid? AirlineId { get; set; }
        public Airline? Airline { get; set; }

        public Guid DepartureCityId { get; set; }
        public City DepartureCity { get; set; } = null!;
        public DateTime DepartureTime { get; set; }

        public Guid ArrivalCityId { get; set; }
        public City ArrivalCity { get; set; } = null!;
        public DateTime ArrivalTime { get; set; }

        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "AZN";

        // Driven by FlightNotificationBackgroundService as departure/arrival
        // times are reached; not set by admins directly.
        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public ICollection<SeatMap> Seats { get; set; } = new List<SeatMap>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}