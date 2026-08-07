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
        public string Currency { get; set; } = "AZN";
        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        // Set when the owning user permanently deletes a cancelled booking from
        // their own "My Tickets" list. Rows are kept (not physically removed) so
        // admin reporting (GET /api/admin/bookings) still shows full booking
        // history even after the user has cleared it from their own view.
        public bool IsDeleted { get; set; } = false;

        public ICollection<BookingPassenger> Passengers { get; set; } = new List<BookingPassenger>();
        public Ticket? Ticket { get; set; }

        // Set by BookingService.CheckInAsync, once the check-in window is open
        // (see Flight.DepartureTime - Flight.CheckInOpensHoursBeforeDeparture)
        // and before it closes (Flight.DepartureTime - Flight.BookingCutoffHours).
        // Distinct from Ticket (issued at booking time, proof of reservation) -
        // BoardingPassCode is the boarding pass, issued at check-in time.
        public bool IsCheckedIn { get; set; } = false;
        public DateTime? CheckedInAt { get; set; }
        public string? BoardingPassCode { get; set; }
    }
}