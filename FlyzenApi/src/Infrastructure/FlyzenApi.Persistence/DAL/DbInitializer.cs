using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.DAL
{
    public static class DbInitializer
    {
        private static readonly (string Name, string Country, string Code)[] CitySeed =
        {
            ("Baku", "Azerbaijan", "GYD"), ("Istanbul", "Turkey", "IST"), ("Dubai", "UAE", "DXB"),
            ("London", "United Kingdom", "LHR"), ("Paris", "France", "CDG"), ("New York", "USA", "JFK"),
            ("Los Angeles", "USA", "LAX"), ("Tokyo", "Japan", "NRT"), ("Beijing", "China", "PEK"),
            ("Sydney", "Australia", "SYD"), ("Berlin", "Germany", "BER"), ("Frankfurt", "Germany", "FRA"),
            ("Amsterdam", "Netherlands", "AMS"), ("Madrid", "Spain", "MAD"), ("Barcelona", "Spain", "BCN"),
            ("Rome", "Italy", "FCO"), ("Milan", "Italy", "MXP"), ("Vienna", "Austria", "VIE"),
            ("Zurich", "Switzerland", "ZRH"), ("Moscow", "Russia", "SVO"), ("Saint Petersburg", "Russia", "LED"),
            ("Tbilisi", "Georgia", "TBS"), ("Yerevan", "Armenia", "EVN"), ("Ankara", "Turkey", "ESB"),
            ("Antalya", "Turkey", "AYT"), ("Abu Dhabi", "UAE", "AUH"), ("Doha", "Qatar", "DOH"),
            ("Riyadh", "Saudi Arabia", "RUH"), ("Kuwait City", "Kuwait", "KWI"), ("Muscat", "Oman", "MCT"),
            ("Cairo", "Egypt", "CAI"), ("Casablanca", "Morocco", "CMN"), ("Lagos", "Nigeria", "LOS"),
            ("Nairobi", "Kenya", "NBO"), ("Mumbai", "India", "BOM"), ("Delhi", "India", "DEL"),
            ("Bangkok", "Thailand", "BKK"), ("Singapore", "Singapore", "SIN"), ("Kuala Lumpur", "Malaysia", "KUL"),
            ("Seoul", "South Korea", "ICN"), ("Hong Kong", "China", "HKG"), ("Toronto", "Canada", "YYZ"),
            ("Vancouver", "Canada", "YVR"), ("Mexico City", "Mexico", "MEX"), ("São Paulo", "Brazil", "GRU"),
            ("Buenos Aires", "Argentina", "EZE"), ("Warsaw", "Poland", "WAW"), ("Prague", "Czech Republic", "PRG"),
            ("Budapest", "Hungary", "BUD"), ("Athens", "Greece", "ATH"),
        };

        private static readonly (string FlightNumber, string From, string To, int DayOffset, int DepHour, int DurationHours, decimal BasePrice)[] FlightSeed =
        {
            ("J2 101", "Baku", "Istanbul", 2, 8, 2, 220m),
            ("EK 202", "New York", "Dubai", 3, 2, 13, 1200m),
            ("TK 303", "Istanbul", "Dubai", 4, 23, 4, 480m),
            ("LH 404", "Baku", "Frankfurt", 5, 14, 5, 650m),
            ("BA 505", "London", "New York", 6, 11, 8, 1100m),
            ("QR 606", "Dubai", "Baku", 7, 15, 4, 310m),
            ("AF 707", "Paris", "Baku", 8, 9, 5, 540m),
            ("EK 808", "Baku", "London", 9, 6, 7, 890m),
            ("SU 909", "Moscow", "Baku", 10, 12, 3, 260m),
            ("TK 111", "Baku", "Antalya", 11, 7, 3, 190m),
        };

        private static readonly (string Code, string Name)[] AirlineSeed =
        {
            ("J2", "Azerbaijan Airlines"),
            ("EK", "Emirates"),
            ("TK", "Turkish Airlines"),
            ("LH", "Lufthansa"),
            ("BA", "British Airways"),
            ("QR", "Qatar Airways"),
            ("AF", "Air France"),
            ("SU", "Aeroflot"),
            ("KE", "Korean Air"),
        };

        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.AnyAsync())
            {
                var hasher = new PasswordHasher<User>();
                var admin = new User
                {
                    FirstName = "Admin",
                    LastName = "Panel",
                    Email = "admin@admin.com",
                    PhoneNumber = "+994 50 000 00 00",
                    Role = UserRole.Admin,
                    IsEmailConfirmed = true,
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

                var demoUser = new User
                {
                    FirstName = "Ahasanul",
                    LastName = "Kabir",
                    Email = "user@user.com",
                    PhoneNumber = "+994 50 123 45 67",
                    Role = UserRole.User,
                    IsEmailConfirmed = true,
                };
                demoUser.PasswordHash = hasher.HashPassword(demoUser, "User123!");

                await context.Users.AddRangeAsync(admin, demoUser);
                await context.SaveChangesAsync();
            }

            if (!await context.Cities.AnyAsync())
            {
                var cities = CitySeed
                    .Select(c => new City { Name = c.Name, Country = c.Country, AirportCode = c.Code, Description = $"{c.Name}, {c.Country}" })
                    .ToList();
                await context.Cities.AddRangeAsync(cities);
                await context.SaveChangesAsync();
            }

            if (!await context.MealOptions.AnyAsync())
            {
                await context.MealOptions.AddRangeAsync(
                    new MealOption { Type = MealType.None, Name = "No Meal", Description = "Skip in-flight meal service.", Price = 0m },
                    new MealOption { Type = MealType.Standard, Name = "Standard Meal", Description = "A balanced meal with meat, vegetables and dessert.", Price = 15m },
                    new MealOption { Type = MealType.Vegetarian, Name = "Vegetarian", Description = "Fresh vegetables, grains, legumes and dairy products.", Price = 12m },
                    new MealOption { Type = MealType.Vegan, Name = "Vegan", Description = "Plant-based only - no meat, fish, dairy or eggs.", Price = 14m },
                    new MealOption { Type = MealType.Halal, Name = "Halal Meal", Description = "Prepared according to Islamic dietary laws.", Price = 13m },
                    new MealOption { Type = MealType.GlutenFree, Name = "Gluten-Free Meal", Description = "No gluten-containing ingredients.", Price = 16m }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.BaggageOptions.AnyAsync())
            {
                await context.BaggageOptions.AddRangeAsync(
                    new BaggageOption { Name = "No extra baggage", WeightKg = 0, Price = 0m, DisplayOrder = 0 },
                    new BaggageOption { Name = "10 kg extra", WeightKg = 10, Price = 30m, DisplayOrder = 1 },
                    new BaggageOption { Name = "20 kg extra", WeightKg = 20, Price = 55m, DisplayOrder = 2 },
                    new BaggageOption { Name = "30 kg extra", WeightKg = 30, Price = 80m, DisplayOrder = 3 },
                    new BaggageOption { Name = "40 kg extra", WeightKg = 40, Price = 110m, DisplayOrder = 4 }
                );
                await context.SaveChangesAsync();
            }

            if (!await context.Airlines.AnyAsync())
            {
                var airlines = AirlineSeed
                    .Select(a => new Airline { Code = a.Code, Name = a.Name })
                    .ToList();
                await context.Airlines.AddRangeAsync(airlines);
                await context.SaveChangesAsync();
            }

            if (!await context.Flights.AnyAsync())
            {
                var citiesByName = await context.Cities.ToDictionaryAsync(c => c.Name, c => c.Id);
                var airlinesByCode = await context.Airlines.ToDictionaryAsync(a => a.Code, a => a.Id);
                var reserved = new HashSet<string> { "A2", "B3", "C4", "D2", "E5", "F1", "G3" };
                var now = DateTime.UtcNow.Date;

                foreach (var f in FlightSeed)
                {
                    if (!citiesByName.TryGetValue(f.From, out var fromId) || !citiesByName.TryGetValue(f.To, out var toId))
                        continue;

                    var airlineCode = f.FlightNumber.Split(' ')[0];
                    airlinesByCode.TryGetValue(airlineCode, out var airlineId);

                    var departure = now.AddDays(f.DayOffset).AddHours(f.DepHour);
                    var flight = new Flight
                    {
                        FlightNumber = f.FlightNumber,
                        AirlineId = airlineId == Guid.Empty ? null : airlineId,
                        DepartureCityId = fromId,
                        ArrivalCityId = toId,
                        DepartureTime = departure,
                        ArrivalTime = departure.AddHours(f.DurationHours),
                        BasePrice = f.BasePrice,
                        Seats = BuildSeatMap(reserved),
                    };
                    await context.Flights.AddAsync(flight);
                }

                await context.SaveChangesAsync();
            }
        }

        private static List<SeatMap> BuildSeatMap(HashSet<string> reserved)
        {
            var seats = new List<SeatMap>();

            foreach (var row in new[] { "A", "B", "C" })
            {
                for (var i = 1; i <= 4; i++)
                {
                    var number = $"{row}{i}";
                    seats.Add(new SeatMap
                    {
                        SeatNumber = number,
                        Class = SeatClass.Business,
                        PriceMultiplier = 1.8m,
                        IsAvailable = !reserved.Contains(number),
                    });
                }
            }

            foreach (var row in new[] { "D", "E", "F", "G" })
            {
                for (var i = 1; i <= 6; i++)
                {
                    var number = $"{row}{i}";
                    seats.Add(new SeatMap
                    {
                        SeatNumber = number,
                        Class = SeatClass.Economy,
                        PriceMultiplier = 1.0m,
                        IsAvailable = !reserved.Contains(number),
                    });
                }
            }

            return seats;
        }
    }
}
