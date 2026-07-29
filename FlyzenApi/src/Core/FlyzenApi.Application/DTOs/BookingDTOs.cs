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
