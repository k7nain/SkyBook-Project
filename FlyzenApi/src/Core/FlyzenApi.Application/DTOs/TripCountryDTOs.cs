using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripCountryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FlagCode { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTripCountryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(2, MinimumLength = 2)]
        public string FlagCode { get; set; } = string.Empty;

        public string? CoverImage { get; set; }
    }

    public class UpdateTripCountryRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(2, MinimumLength = 2)]
        public string FlagCode { get; set; } = string.Empty;

        public string? CoverImage { get; set; }
    }
}
