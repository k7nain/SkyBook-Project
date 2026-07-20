using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class SeatMapConfiguration : IEntityTypeConfiguration<SeatMap>
    {
        public void Configure(EntityTypeBuilder<SeatMap> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.SeatNumber).IsRequired().HasMaxLength(10);
            builder.HasOne(x => x.Flight).WithMany(f => f.Seats).HasForeignKey(x => x.FlightId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
