using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripCountryDto
    {
        public Guid Id { get; set; }
        public string NameAz { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string FlagCode { get; set; } = string.Empty;
        public string? CoverImage { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTripCountryRequest
    {
        [Required, MaxLength(100)]
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameEn { get; set; }

        [MaxLength(100)]
        public string? NameRu { get; set; }

        [Required, StringLength(2, MinimumLength = 2)]
        public string FlagCode { get; set; } = string.Empty;

        public string? CoverImage { get; set; }
    }

    public class UpdateTripCountryRequest
    {
        [Required, MaxLength(100)]
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameEn { get; set; }

        [MaxLength(100)]
        public string? NameRu { get; set; }

        [Required, StringLength(2, MinimumLength = 2)]
        public string FlagCode { get; set; } = string.Empty;

        public string? CoverImage { get; set; }
    }
}
