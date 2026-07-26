using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripCityDto
    {
        public Guid Id { get; set; }
        public Guid CountryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Image { get; set; }
        public string? ShortDescription { get; set; }
    }

    public class CreateTripCityRequest
    {
        [Required]
        public Guid CountryId { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }
    }

    public class UpdateTripCityRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public string? Image { get; set; }

        [MaxLength(500)]
        public string? ShortDescription { get; set; }
    }
}
