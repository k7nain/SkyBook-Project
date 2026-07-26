using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripPlaceDto
    {
        public Guid Id { get; set; }
        public Guid CityId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TripPlaceImageDto
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class CreateTripPlaceRequest
    {
        [Required]
        public Guid CityId { get; set; }

        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }

    public class UpdateTripPlaceRequest
    {
        [Required, MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Category { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }
    }

    public class AddTripPlaceImageRequest
    {
        [Required]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
