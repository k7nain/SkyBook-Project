using System;
using FlyzenApi.Domain.Entities.Common;

namespace FlyzenApi.Domain.Entities
{
    public class CityGalleryImage : BaseEntity
    {
        public Guid CityId { get; set; }
        public City City { get; set; } = null!;
        public string ImageUrl { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}