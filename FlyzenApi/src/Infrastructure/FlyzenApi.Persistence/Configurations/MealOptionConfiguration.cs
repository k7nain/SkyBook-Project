using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlyzenApi.Persistence.Configurations
{
    public class MealOptionConfiguration : IEntityTypeConfiguration<MealOption>
    {
        public void Configure(EntityTypeBuilder<MealOption> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Price).HasPrecision(10, 2);
        }
    }
}
