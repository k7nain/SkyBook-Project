using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class BookingPassengerConfiguration : IEntityTypeConfiguration<BookingPassenger>
    {
        public void Configure(EntityTypeBuilder<BookingPassenger> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.LastName).IsRequired().HasMaxLength(100);
            builder.HasOne(x => x.Booking).WithMany(b => b.Passengers).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Seat).WithMany().HasForeignKey(x => x.SeatId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.MealOption).WithMany().HasForeignKey(x => x.MealOptionId).OnDelete(DeleteBehavior.SetNull);
            builder.HasOne(x => x.BaggageOption).WithMany().HasForeignKey(x => x.BaggageOptionId).OnDelete(DeleteBehavior.SetNull);
        }
    }
}
