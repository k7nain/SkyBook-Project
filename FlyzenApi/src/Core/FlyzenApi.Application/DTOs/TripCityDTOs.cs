using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripCityDto
    {
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        public string NameAz { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string? Image { get; set; }
        public string? ShortDescriptionAz { get; set; }
        public string? ShortDescriptionEn { get; set; }
        public string? ShortDescriptionRu { get; set; }
    }

    public class CreateTripCityRequest
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required, MaxLength(100)]
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameEn { get; set; }

        [MaxLength(100)]
        public string? NameRu { get; set; }

        public string? Image { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionAz { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionEn { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionRu { get; set; }
    }

    public class UpdateTripCityRequest
    {
        [Required, MaxLength(100)]
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? NameEn { get; set; }

        [MaxLength(100)]
        public string? NameRu { get; set; }

        public string? Image { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionAz { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionEn { get; set; }

        [MaxLength(500)]
        public string? ShortDescriptionRu { get; set; }
    }
}
