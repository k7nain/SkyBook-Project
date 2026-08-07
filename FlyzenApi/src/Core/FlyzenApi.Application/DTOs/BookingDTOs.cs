using System.ComponentModel.DataAnnotations;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class PassengerSelectionRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PassportNumber { get; set; } = string.Empty;

        public PassengerType Type { get; set; } = PassengerType.Adult;

        [Required]
        public Guid SeatId { get; set; }

        public Guid? MealOptionId { get; set; }
        public Guid? BaggageOptionId { get; set; }
    }

    public class CreateBookingRequest
    {
        [Required]
        public Guid FlightId { get; set; }

        [Required, MinLength(1)]
        public List<PassengerSelectionRequest> Passengers { get; set; } = new();

        public string? PromoCode { get; set; }

        // Stacks with PromoCode (applied after it - see BookingService.CreateAsync).
        // Preview-only client-side; re-validated and clamped from scratch here,
        // same principle as PromoCode never being trusted from the client.
        public int? SkyPointsToRedeem { get; set; }
    }

    public class BookingPassengerDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PassportNumber { get; set; } = string.Empty;
        public PassengerType Type { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string? MealName { get; set; }
        public string? BaggageLabel { get; set; }
        public decimal PriceCalculated { get; set; }
    }

    public class TicketDto
    {
        public string TicketNumber { get; set; } = string.Empty;
        public string QrCodeData { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; }
    }

    public class BookingDto
    {
        public Guid Id { get; set; }
        public string PNR { get; set; } = string.Empty;
        public BookingStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public string Currency { get; set; } = "AZN";
        public FlightSummaryDto Flight { get; set; } = null!;
        public List<BookingPassengerDto> Passengers { get; set; } = new();
        public TicketDto? Ticket { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }

    public class CheckInStatusDto
    {
        public CheckInAvailability Availability { get; set; }

        // Always populated regardless of Availability, so the client can show
        // "opens at X" before the window and "was open until X" after it closes,
        // not just a bare status enum.
        public DateTime OpensAtUtc { get; set; }
        public DateTime ClosesAtUtc { get; set; }
        public DateTime? CheckedInAt { get; set; }
    }

    public class BoardingPassDto
    {
        public Guid BookingId { get; set; }
        public string PNR { get; set; } = string.Empty;
        public string PassengerName { get; set; } = string.Empty;
        public string FlightNumber { get; set; } = string.Empty;
        public string DepartureCityName { get; set; } = string.Empty;
        public string DepartureAirportCode { get; set; } = string.Empty;
        public string ArrivalCityName { get; set; } = string.Empty;
        public string ArrivalAirportCode { get; set; } = string.Empty;
        // UTC - the client renders it in the departure city's local time using
        // Flight's own DepartureCity.TimeZoneId, same convention as every other
        // departure-time display in this app (see services/mappers.js).
        public DateTime DepartureTimeUtc { get; set; }
        public string? Gate { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public string SeatClass { get; set; } = string.Empty;

        public string BoardingPassCode { get; set; } = string.Empty;
        // What the QR code should actually encode - decided server-side so the
        // client never has to know or reconstruct the payload format itself.
        public string QrPayload { get; set; } = string.Empty;
    }

    public class AdminBookingDto : BookingDto
    {
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
    }

    public class UpdateBookingStatusRequest
    {
        [Required]
        public BookingStatus Status { get; set; }
    }

    public class SendTicketEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class AdminUserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
