using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class BaggageOptionConfiguration : IEntityTypeConfiguration<BaggageOption>
    {
        public void Configure(EntityTypeBuilder<BaggageOption> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Price).HasPrecision(10, 2);
        }
    }
}
