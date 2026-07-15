using FlyzenApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FlyzenApi.Persistence.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<CityGalleryImage> CityGalleryImages { get; set; }
        public DbSet<Flight> Flights { get; set; }
        public DbSet<SeatMap> SeatMaps { get; set; }
        public DbSet<MealOption> MealOptions { get; set; }
        public DbSet<BaggageOption> BaggageOptions { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingPassenger> BookingPassengers { get; set; }
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
