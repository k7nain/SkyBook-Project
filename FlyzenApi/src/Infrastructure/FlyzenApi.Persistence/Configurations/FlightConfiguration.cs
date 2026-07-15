using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class FlightConfiguration : IEntityTypeConfiguration<Flight>
    {
        public void Configure(EntityTypeBuilder<Flight> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasOne(x => x.DepartureCity).WithMany(c => c.FlightsFromHere).HasForeignKey(x => x.DepartureCityId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.ArrivalCity).WithMany(c => c.FlightsToHere).HasForeignKey(x => x.ArrivalCityId).OnDelete(DeleteBehavior.Restrict);
        }
    }
}