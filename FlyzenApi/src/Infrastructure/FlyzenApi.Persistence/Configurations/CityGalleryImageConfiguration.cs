using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class CityGalleryImageConfiguration : IEntityTypeConfiguration<CityGalleryImage>
    {
        public void Configure(EntityTypeBuilder<CityGalleryImage> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ImageUrl).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
        }
    }
}
