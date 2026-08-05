using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class Flight : BaseEntity
    {
        // Standard airline booking cutoff: a flight stops being searchable/
        // bookable once we're within this many hours of its departure, not
        // just once it has actually departed.
        public const int BookingCutoffHours = 1;

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

        // Set manually by the admin per flight, like BasePrice - never
        // computed by the system (no automatic price-based formula).
        // Awarded flat (once per booking, not multiplied by passenger count)
        // when a booking on this flight is created - see BookingService.
        public int SkyPoints { get; set; } = 0;

        // Driven by FlightNotificationBackgroundService as departure/arrival
        // times are reached; not set by admins directly.
        public FlightStatus Status { get; set; } = FlightStatus.Scheduled;

        public ICollection<SeatMap> Seats { get; set; } = new List<SeatMap>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}