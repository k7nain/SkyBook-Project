using System.ComponentModel.DataAnnotations;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class FlightSummaryDto
    {
        public Guid Id { get; set; }
        public string FlightNumber { get; set; } = string.Empty;
        public AirlineDto? Airline { get; set; }
        public CityDto DepartureCity { get; set; } = null!;
        public DateTime DepartureTime { get; set; }
        public CityDto ArrivalCity { get; set; } = null!;
        public DateTime ArrivalTime { get; set; }
        public decimal BasePrice { get; set; }
        public string Currency { get; set; } = "AZN";
        public int AvailableSeats { get; set; }
        public int SkyPoints { get; set; }
    }

    public class SeatDto
    {
        public Guid Id { get; set; }
        public string SeatNumber { get; set; } = string.Empty;
        public SeatClass Class { get; set; }
        public bool IsAvailable { get; set; }
        public decimal PriceMultiplier { get; set; }
        public decimal Price { get; set; }
    }

    public class FlightDetailDto : FlightSummaryDto
    {
        public List<SeatDto> Seats { get; set; } = new();
    }

    public class CreateFlightRequest
    {
        [Required, MaxLength(20)]
        public string FlightNumber { get; set; } = string.Empty;

        [Required]
        public Guid AirlineId { get; set; }

        [Required]
        public Guid DepartureCityId { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public Guid ArrivalCityId { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        // The amount is interpreted in Currency (defaults to AZN, the app's base
        // currency, if omitted) and converted to AZN via ICurrencyConversionService
        // before being stored - Flight.BasePrice/Currency in the database are
        // always the converted AZN amount, never the admin's input currency.
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be greater than zero.")]
        public decimal BasePrice { get; set; }

        public string Currency { get; set; } = "AZN";

        // Set manually by the admin, like BasePrice - never computed from price
        // or anything else by the system. See Flight.SkyPoints.
        [Range(0, int.MaxValue)]
        public int SkyPoints { get; set; }
    }

    public class UpdateFlightPriceRequest
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "BasePrice must be greater than zero.")]
        public decimal BasePrice { get; set; }

        // Same convert-then-store-as-AZN contract as CreateFlightRequest.
        public string Currency { get; set; } = "AZN";

        // Nullable despite the DTO's price-focused name/route - this is also
        // the only "edit an existing flight" endpoint that exists, so it
        // doubles as the Sky Points quick-edit. Null means "leave unchanged".
        [Range(0, int.MaxValue)]
        public int? SkyPoints { get; set; }
    }

    public class MealOptionDto
    {
        public Guid Id { get; set; }
        public MealType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Currency { get; set; } = "AZN";
    }

    public class BaggageOptionDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int WeightKg { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; } = "AZN";
        public int DisplayOrder { get; set; }
    }
}
