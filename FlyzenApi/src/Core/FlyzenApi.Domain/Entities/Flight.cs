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
        // just once it has actually departed. Online check-in also CLOSES at
        // this same cutoff (see BookingService.GetCheckInStatusAsync) - by
        // design, not coincidence: once a flight can no longer be booked,
        // there's no reason check-in should still be open either.
        public const int BookingCutoffHours = 1;

        // Online check-in conventionally opens this many hours before departure.
        // Single source of truth for both BookingService's actual check-in
        // window (Application layer, which can't depend on the Infrastructure-
        // layer NotificationOptions the CheckInOpen reminder job reads) and
        // FlightNotificationBackgroundService.ProcessCheckInAsync's reminder
        // timing - kept as one Domain constant specifically so those two can
        // never silently drift out of sync with each other.
        public const int CheckInOpensHoursBeforeDeparture = 24;

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

        // Both admin-settable via AdminService.UpdateFlightOperationalStatusAsync,
        // unlike Status above. Changing either notifies affected travelers
        // (GateChanged / FlightDelayed / BoardingReminder).
        public string? GateNumber { get; set; }
        public FlightOperationalStatus OperationalStatus { get; set; } = FlightOperationalStatus.Normal;

        public ICollection<SeatMap> Seats { get; set; } = new List<SeatMap>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}