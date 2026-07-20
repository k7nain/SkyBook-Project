using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class CityConfiguration : IEntityTypeConfiguration<City>
    {
        public void Configure(EntityTypeBuilder<City> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Country).IsRequired().HasMaxLength(100);
            builder.Property(x => x.AirportCode).IsRequired().HasMaxLength(10);
            builder.HasIndex(x => x.AirportCode).IsUnique();
            builder.HasMany(x => x.GalleryImages).WithOne(g => g.City).HasForeignKey(g => g.CityId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
