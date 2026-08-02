using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FlyzenApi.Persistence.DAL
{
    /// Lets `dotnet ef` build AppDbContext directly for migration generation,
    /// without booting the full API host (and its startup seeding/migration logic).
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5433;Database=skybook;Username=skybook;Password=skybook_dev_password");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
