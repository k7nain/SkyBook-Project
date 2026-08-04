using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class TripPlaceDto
    {
        public Guid Id { get; set; }
        public Guid CityId { get; set; }
        public string NameAz { get; set; } = string.Empty;
        public string? NameEn { get; set; }
        public string? NameRu { get; set; }
        public string? DescriptionAz { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DescriptionRu { get; set; }
        public string? Category { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
    }

    public class TripPlaceImageDto
    {
        public Guid Id { get; set; }
        public Guid PlaceId { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class CreateTripPlaceRequest
    {
        [Required]
        public Guid CityId { get; set; }

        [Required, MaxLength(150)]
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? NameEn { get; set; }

        [MaxLength(150)]
        public string? NameRu { get; set; }

        [MaxLength(2000)]
        public string? DescriptionAz { get; set; }

        [MaxLength(2000)]
        public string? DescriptionEn { get; set; }

        [MaxLength(2000)]
        public string? DescriptionRu { get; set; }

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
        public string NameAz { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? NameEn { get; set; }

        [MaxLength(150)]
        public string? NameRu { get; set; }

        [MaxLength(2000)]
        public string? DescriptionAz { get; set; }

        [MaxLength(2000)]
        public string? DescriptionEn { get; set; }

        [MaxLength(2000)]
        public string? DescriptionRu { get; set; }

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

    public class ReorderTripPlaceGalleryRequest
    {
        [Required]
        public List<Guid> ImageIds { get; set; } = new();
    }
}
