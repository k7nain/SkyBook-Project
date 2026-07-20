using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
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
            
            // Seed data
            SeedData(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Cities
            var city1 = new City
            {
                Id = Guid.NewGuid(),
                Name = "Istanbul",
                Country = "Turkey",
                AirportCode = "IST",
                Description = "Istanbul is the largest city in Turkey and a major commercial center.",
                CreatedAt = DateTime.UtcNow
            };

            var city2 = new City
            {
                Id = Guid.NewGuid(),
                Name = "Dubai",
                Country = "United Arab Emirates",
                AirportCode = "DXB",
                Description = "Dubai is a global city and business hub in the Middle East.",
                CreatedAt = DateTime.UtcNow
            };

            var city3 = new City
            {
                Id = Guid.NewGuid(),
                Name = "Baku",
                Country = "Azerbaijan",
                AirportCode = "GYD",
                Description = "Baku is the capital and largest city of Azerbaijan.",
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<City>().HasData(city1, city2, city3);

            // City Gallery Images
            modelBuilder.Entity<CityGalleryImage>().HasData(
                new CityGalleryImage { Id = Guid.NewGuid(), CityId = city1.Id, ImageUrl = "https://images.unsplash.com/photo-1524678606370-a47ad25cb82a?w=500", Description = "Istanbul Skyline", CreatedAt = DateTime.UtcNow },
                new CityGalleryImage { Id = Guid.NewGuid(), CityId = city2.Id, ImageUrl = "https://images.unsplash.com/photo-1512453335684-cf7fdd84efb7?w=500", Description = "Dubai Marina", CreatedAt = DateTime.UtcNow },
                new CityGalleryImage { Id = Guid.NewGuid(), CityId = city3.Id, ImageUrl = "https://images.unsplash.com/photo-1604516453874-85c03432c262?w=500", Description = "Baku at Night", CreatedAt = DateTime.UtcNow }
            );

            // Meal Options
            var mealOptions = new[]
            {
                new MealOption { Id = Guid.NewGuid(), Name = "Standard Meal", Type = MealType.Standard, Price = 0, CreatedAt = DateTime.UtcNow },
                new MealOption { Id = Guid.NewGuid(), Name = "Vegetarian Meal", Type = MealType.Vegetarian, Price = 15, CreatedAt = DateTime.UtcNow },
                new MealOption { Id = Guid.NewGuid(), Name = "Vegan Meal", Type = MealType.Vegan, Price = 20, CreatedAt = DateTime.UtcNow },
                new MealOption { Id = Guid.NewGuid(), Name = "Halal Meal", Type = MealType.Halal, Price = 18, CreatedAt = DateTime.UtcNow },
                new MealOption { Id = Guid.NewGuid(), Name = "Gluten Free Meal", Type = MealType.GlutenFree, Price = 25, CreatedAt = DateTime.UtcNow }
            };
            modelBuilder.Entity<MealOption>().HasData(mealOptions);

            // Baggage Options
            var baggageOptions = new[]
            {
                new BaggageOption { Id = Guid.NewGuid(), Name = "Standard Baggage (20kg)", Price = 0, CreatedAt = DateTime.UtcNow },
                new BaggageOption { Id = Guid.NewGuid(), Name = "Extra Baggage (20kg)", Price = 30, CreatedAt = DateTime.UtcNow },
                new BaggageOption { Id = Guid.NewGuid(), Name = "Premium Baggage (30kg)", Price = 50, CreatedAt = DateTime.UtcNow }
            };
            modelBuilder.Entity<BaggageOption>().HasData(baggageOptions);

            // Flights
            var flight1 = new Flight
            {
                Id = Guid.NewGuid(),
                FlightNumber = "TK101",
                DepartureCityId = city1.Id,
                ArrivalCityId = city2.Id,
                DepartureTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10),
                ArrivalTime = DateTime.UtcNow.AddDays(1).Date.AddHours(15),
                BasePrice = 150,
                CreatedAt = DateTime.UtcNow
            };

            var flight2 = new Flight
            {
                Id = Guid.NewGuid(),
                FlightNumber = "TK102",
                DepartureCityId = city2.Id,
                ArrivalCityId = city3.Id,
                DepartureTime = DateTime.UtcNow.AddDays(2).Date.AddHours(09),
                ArrivalTime = DateTime.UtcNow.AddDays(2).Date.AddHours(12),
                BasePrice = 120,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<Flight>().HasData(flight1, flight2);

            // Seats for Flight 1
            var seats = new List<SeatMap>();
            var seatClasses = new[] { SeatClass.Economy, SeatClass.Business };

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    var seatClass = i < 2 ? SeatClass.Business : SeatClass.Economy;
                    seats.Add(new SeatMap
                    {
                        Id = Guid.NewGuid(),
                        FlightId = flight1.Id,
                        SeatNumber = $"{(char)('A' + j)}{i + 1}",
                        Class = seatClass,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    var seatClass = SeatClass.Economy;
                    seats.Add(new SeatMap
                    {
                        Id = Guid.NewGuid(),
                        FlightId = flight2.Id,
                        SeatNumber = $"{(char)('A' + j)}{i + 1}",
                        Class = seatClass,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            modelBuilder.Entity<SeatMap>().HasData(seats);
        }
    }
}
