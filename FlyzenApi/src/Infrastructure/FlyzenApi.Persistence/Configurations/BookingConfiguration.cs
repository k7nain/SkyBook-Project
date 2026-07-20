using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PNR).IsRequired().HasMaxLength(10);
            builder.HasIndex(x => x.PNR).IsUnique();
            builder.HasOne(x => x.User).WithMany(u => u.Bookings).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Flight).WithMany(f => f.Bookings).HasForeignKey(x => x.FlightId).OnDelete(DeleteBehavior.Restrict);
            builder.HasMany(x => x.Passengers).WithOne(bp => bp.Booking).HasForeignKey(bp => bp.BookingId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
