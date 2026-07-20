using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class TicketConfiguration : IEntityTypeConfiguration<Ticket>
    {
        public void Configure(EntityTypeBuilder<Ticket> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TicketNumber).IsRequired().HasMaxLength(20);
            builder.HasIndex(x => x.TicketNumber).IsUnique();
            builder.Property(x => x.QrCodeData).HasMaxLength(500);
            builder.HasOne(x => x.Booking).WithOne(b => b.Ticket).HasForeignKey<Ticket>(t => t.BookingId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
