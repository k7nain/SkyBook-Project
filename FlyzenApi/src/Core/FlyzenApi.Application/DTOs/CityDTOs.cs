using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class CityDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string AirportCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Climate { get; set; }
        public string? Attractions { get; set; }
    }

    public class CityGalleryImageDto
    {
        public Guid Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class UpdateCityRequest
    {
        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? Climate { get; set; }

        [MaxLength(1000)]
        public string? Attractions { get; set; }
    }

    public class AddCityGalleryImageRequest
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;

        public string? Description { get; set; }
    }
}
