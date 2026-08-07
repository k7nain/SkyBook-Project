using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlyzenApi.Persistence.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            StampTimestamps();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        {
            StampTimestamps();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        private void StampTimestamps()
        {
            var now = DateTime.UtcNow;
            foreach (var entry in ChangeTracker.Entries<BaseEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                }
            }
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<City> Cities => Set<City>();
        public DbSet<Airline> Airlines => Set<Airline>();
        public DbSet<CityGalleryImage> CityGalleryImages => Set<CityGalleryImage>();
        public DbSet<Flight> Flights => Set<Flight>();
        public DbSet<SeatMap> Seats => Set<SeatMap>();
        public DbSet<MealOption> MealOptions => Set<MealOption>();
        public DbSet<BaggageOption> BaggageOptions => Set<BaggageOption>();
        public DbSet<Booking> Bookings => Set<Booking>();
        public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<TripCountry> TripCountries => Set<TripCountry>();
        public DbSet<TripCity> TripCities => Set<TripCity>();
        public DbSet<TripPlace> TripPlaces => Set<TripPlace>();
        public DbSet<TripPlaceImage> TripPlaceImages => Set<TripPlaceImage>();
        public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<BookingReminder> BookingReminders => Set<BookingReminder>();
        public DbSet<FlightNotificationLog> FlightNotificationLogs => Set<FlightNotificationLog>();
        public DbSet<SkyPointsTransaction> SkyPointsTransactions => Set<SkyPointsTransaction>();
        public DbSet<TravelJournal> TravelJournals => Set<TravelJournal>();
        public DbSet<TravelJournalImage> TravelJournalImages => Set<TravelJournalImage>();
        public DbSet<SearchLog> SearchLogs => Set<SearchLog>();
        public DbSet<UserRecommendationCache> UserRecommendationCaches => Set<UserRecommendationCache>();
        public DbSet<PriceProposal> PriceProposals => Set<PriceProposal>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).IsRequired().HasMaxLength(256);
                b.Property(u => u.FirstName).IsRequired().HasMaxLength(100);
                b.Property(u => u.LastName).IsRequired().HasMaxLength(100);

                b.HasIndex(u => u.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");
                b.HasIndex(u => u.AppleId).IsUnique().HasFilter("\"AppleId\" IS NOT NULL");
                b.HasIndex(u => u.PasswordResetTokenHash);
                b.Property(u => u.PreferredTimezone).HasMaxLength(64);
                b.Property(u => u.SkyPointsBalance).HasDefaultValue(0);
            });

            modelBuilder.Entity<City>(b =>
            {
                b.Property(c => c.Name).IsRequired().HasMaxLength(100);
                b.Property(c => c.AirportCode).IsRequired().HasMaxLength(10);
                b.Property(c => c.Country).IsRequired().HasMaxLength(100);
                b.Property(c => c.TimeZoneId).IsRequired().HasMaxLength(64).HasDefaultValue("UTC");

                b.HasMany(c => c.GalleryImages)
                    .WithOne(g => g.City)
                    .HasForeignKey(g => g.CityId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(c => c.FlightsFromHere)
                    .WithOne(f => f.DepartureCity)
                    .HasForeignKey(f => f.DepartureCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(c => c.FlightsToHere)
                    .WithOne(f => f.ArrivalCity)
                    .HasForeignKey(f => f.ArrivalCityId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Airline>(b =>
            {
                b.Property(a => a.Name).IsRequired().HasMaxLength(100);
                b.Property(a => a.Code).IsRequired().HasMaxLength(10);
                b.HasIndex(a => a.Code).IsUnique();
            });

            modelBuilder.Entity<Flight>(b =>
            {
                b.Property(f => f.FlightNumber).IsRequired().HasMaxLength(20);
                b.Property(f => f.BasePrice).HasPrecision(18, 2);
                b.Property(f => f.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("AZN");
                b.Property(f => f.GateNumber).HasMaxLength(10);
                b.Property(f => f.OperationalStatus).HasDefaultValue(FlightOperationalStatus.Normal);

                b.HasMany(f => f.Seats)
                    .WithOne(s => s.Flight)
                    .HasForeignKey(s => s.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasMany(f => f.Bookings)
                    .WithOne(bk => bk.Flight)
                    .HasForeignKey(bk => bk.FlightId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Nullable so flights created before airlines existed as a concept
                // remain valid; the frontend falls back to deriving a display name
                // from the flight number for those.
                b.HasOne(f => f.Airline)
                    .WithMany(a => a.Flights)
                    .HasForeignKey(f => f.AirlineId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SeatMap>(b =>
            {
                b.Property(s => s.SeatNumber).IsRequired().HasMaxLength(10);
                b.Property(s => s.PriceMultiplier).HasPrecision(5, 2);
                b.HasIndex(s => new { s.FlightId, s.SeatNumber }).IsUnique();
                // Optimistic concurrency on Postgres' hidden system column: guards against
                // two concurrent bookings both reading IsAvailable=true for the same seat.
                b.Property<uint>("xmin").IsRowVersion();

                b.HasOne(s => s.Booking)
                    .WithMany()
                    .HasForeignKey(s => s.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<MealOption>(b =>
            {
                b.Property(m => m.Name).IsRequired().HasMaxLength(100);
                b.Property(m => m.Price).HasPrecision(18, 2);
                b.Property(m => m.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("AZN");
            });

            modelBuilder.Entity<BaggageOption>(b =>
            {
                b.Property(o => o.Name).IsRequired().HasMaxLength(100);
                b.Property(o => o.Price).HasPrecision(18, 2);
                b.Property(o => o.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("AZN");
            });

            modelBuilder.Entity<Booking>(b =>
            {
                b.Property(bk => bk.PNR).IsRequired().HasMaxLength(10);
                b.Property(bk => bk.TotalPrice).HasPrecision(18, 2);
                b.Property(bk => bk.Currency).IsRequired().HasMaxLength(3).HasDefaultValue("AZN");
                b.Property(bk => bk.IsDeleted).HasDefaultValue(false);
                b.Property(bk => bk.IsCheckedIn).HasDefaultValue(false);
                b.Property(bk => bk.BoardingPassCode).HasMaxLength(20);
                b.HasIndex(bk => bk.PNR).IsUnique();
                // Partial (most bookings never check in, so most rows are null -
                // a plain unique index would still work on Postgres since it
                // already treats NULL as distinct, but the filter makes that
                // "only real codes are unique" intent explicit).
                b.HasIndex(bk => bk.BoardingPassCode).IsUnique().HasFilter("\"BoardingPassCode\" IS NOT NULL");

                b.HasOne(bk => bk.User)
                    .WithMany(u => u.Bookings)
                    .HasForeignKey(bk => bk.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(bk => bk.Passengers)
                    .WithOne(p => p.Booking)
                    .HasForeignKey(p => p.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(bk => bk.Ticket)
                    .WithOne(t => t.Booking)
                    .HasForeignKey<Ticket>(t => t.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<BookingPassenger>(b =>
            {
                b.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
                b.Property(p => p.LastName).IsRequired().HasMaxLength(100);
                b.Property(p => p.PassportNumber).IsRequired().HasMaxLength(30);
                b.Property(p => p.PriceCalculated).HasPrecision(18, 2);

                b.HasOne(p => p.Seat)
                    .WithMany()
                    .HasForeignKey(p => p.SeatMapId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(p => p.MealOption)
                    .WithMany()
                    .HasForeignKey(p => p.MealOptionId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(p => p.BaggageOption)
                    .WithMany()
                    .HasForeignKey(p => p.BaggageOptionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Ticket>(b =>
            {
                b.Property(t => t.TicketNumber).IsRequired().HasMaxLength(20);
                b.Property(t => t.QrCodeData).IsRequired();
                b.HasIndex(t => t.TicketNumber).IsUnique();
            });

            modelBuilder.Entity<TripCountry>(b =>
            {
                b.Property(c => c.NameAz).IsRequired().HasMaxLength(100);
                b.Property(c => c.NameEn).HasMaxLength(100);
                b.Property(c => c.NameRu).HasMaxLength(100);
                b.Property(c => c.FlagCode).IsRequired().HasMaxLength(2);

                b.HasMany(c => c.Cities)
                    .WithOne(ci => ci.Country)
                    .HasForeignKey(ci => ci.CountryId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TripCity>(b =>
            {
                b.Property(c => c.NameAz).IsRequired().HasMaxLength(100);
                b.Property(c => c.NameEn).HasMaxLength(100);
                b.Property(c => c.NameRu).HasMaxLength(100);
                b.Property(c => c.ShortDescriptionAz).HasMaxLength(500);
                b.Property(c => c.ShortDescriptionEn).HasMaxLength(500);
                b.Property(c => c.ShortDescriptionRu).HasMaxLength(500);

                b.HasMany(c => c.Places)
                    .WithOne(p => p.City)
                    .HasForeignKey(p => p.CityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TripPlace>(b =>
            {
                b.Property(p => p.NameAz).IsRequired().HasMaxLength(150);
                b.Property(p => p.NameEn).HasMaxLength(150);
                b.Property(p => p.NameRu).HasMaxLength(150);
                b.Property(p => p.DescriptionAz).HasMaxLength(2000);
                b.Property(p => p.DescriptionEn).HasMaxLength(2000);
                b.Property(p => p.DescriptionRu).HasMaxLength(2000);
                b.Property(p => p.Category).HasMaxLength(50);

                b.HasMany(p => p.Images)
                    .WithOne(i => i.Place)
                    .HasForeignKey(i => i.PlaceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PromoCode>(b =>
            {
                b.Property(p => p.Code).IsRequired().HasMaxLength(30);
                b.Property(p => p.DiscountPercentage).HasPrecision(5, 2);
                b.HasIndex(p => p.Code).IsUnique();
                // Optimistic concurrency: guards against two concurrent bookings both
                // reading UsedCount < MaxUses for the same near-exhausted code.
                b.Property<uint>("xmin").IsRowVersion();
            });

            modelBuilder.Entity<Notification>(b =>
            {
                b.Property(n => n.Title).IsRequired().HasMaxLength(200);
                b.Property(n => n.Message).IsRequired().HasMaxLength(1000);
                b.Property(n => n.TitleKey).HasMaxLength(200);
                b.Property(n => n.BodyKey).HasMaxLength(200);
                b.Property(n => n.ParamsJson).HasMaxLength(1000);
                b.HasIndex(n => n.UserId);

                b.HasOne(n => n.User)
                    .WithMany()
                    .HasForeignKey(n => n.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(n => n.Booking)
                    .WithMany()
                    .HasForeignKey(n => n.BookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SkyPointsTransaction>(b =>
            {
                b.HasIndex(t => t.UserId);

                b.HasOne(t => t.User)
                    .WithMany()
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // SetNull (not Restrict/Cascade): a transaction row is a
                // permanent history/audit entry - it must survive even if the
                // related booking is later hard-deleted (Booking.IsDeleted is
                // normally a soft-delete, but this keeps the ledger safe either way).
                b.HasOne(t => t.RelatedBooking)
                    .WithMany()
                    .HasForeignKey(t => t.RelatedBookingId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<BookingReminder>(b =>
            {
                b.HasIndex(r => new { r.BookingId, r.DaysBefore }).IsUnique();

                b.HasOne(r => r.Booking)
                    .WithMany()
                    .HasForeignKey(r => r.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FlightNotificationLog>(b =>
            {
                b.HasIndex(l => new { l.FlightId, l.EventType }).IsUnique();

                b.HasOne(l => l.Flight)
                    .WithMany()
                    .HasForeignKey(l => l.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TravelJournal>(b =>
            {
                b.Property(j => j.Title).IsRequired().HasMaxLength(150);
                b.Property(j => j.Body).IsRequired().HasMaxLength(3000);
                b.Property(j => j.IsPublished).HasDefaultValue(true);
                b.HasIndex(j => j.DestinationCityId);

                b.HasOne(j => j.User)
                    .WithMany()
                    .HasForeignKey(j => j.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Cascade (not Restrict): a journal only exists to describe one
                // specific booking's trip - if that booking is ever hard-deleted
                // (account deletion), the journal has nothing left to be "verified"
                // against and should go with it.
                b.HasOne(j => j.Booking)
                    .WithMany()
                    .HasForeignKey(j => j.BookingId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(j => j.DestinationCity)
                    .WithMany()
                    .HasForeignKey(j => j.DestinationCityId)
                    .OnDelete(DeleteBehavior.Restrict);

                b.HasMany(j => j.Images)
                    .WithOne(i => i.Journal)
                    .HasForeignKey(i => i.JournalId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TravelJournalImage>(b =>
            {
                b.Property(i => i.ImageUrl).IsRequired();
            });

            modelBuilder.Entity<SearchLog>(b =>
            {
                b.HasIndex(l => new { l.UserId, l.CreatedAt });

                b.HasOne(l => l.User)
                    .WithMany()
                    .HasForeignKey(l => l.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(l => l.FromCity)
                    .WithMany()
                    .HasForeignKey(l => l.FromCityId)
                    .OnDelete(DeleteBehavior.SetNull);

                b.HasOne(l => l.ToCity)
                    .WithMany()
                    .HasForeignKey(l => l.ToCityId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserRecommendationCache>(b =>
            {
                b.Property(c => c.ResponseJson).IsRequired();
                b.Property(c => c.SignatureHash).IsRequired().HasMaxLength(2000);
                b.HasIndex(c => c.UserId).IsUnique();

                b.HasOne(c => c.User)
                    .WithMany()
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<PriceProposal>(b =>
            {
                b.Property(p => p.CurrentPrice).HasPrecision(18, 2);
                b.Property(p => p.SuggestedPrice).HasPrecision(18, 2);
                b.Property(p => p.OccupancyRate).HasPrecision(5, 4);
                b.Property(p => p.VelocityFactor).HasPrecision(5, 4);
                b.Property(p => p.UrgencyFactor).HasPrecision(5, 4);
                b.Property(p => p.DemandScore).HasPrecision(5, 4);

                // At most one PENDING proposal per flight at a time - the job
                // waits for an admin to decide on an existing one before it can
                // propose again for that same flight (see DynamicPricingBackgroundService).
                // The raw "= 0" is PriceProposalStatus.Pending's stored int value -
                // a Postgres filtered index predicate can't reference a C# enum
                // member, only the literal it's stored as. If Pending ever stops
                // being the first value in that enum, this filter must be updated
                // to match (values are appended, not reordered, everywhere else in
                // this codebase specifically to avoid this kind of drift).
                b.HasIndex(p => p.FlightId)
                    .IsUnique()
                    .HasFilter($"\"{nameof(PriceProposal.Status)}\" = 0");

                b.HasOne(p => p.Flight)
                    .WithMany()
                    .HasForeignKey(p => p.FlightId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(p => p.DecidedByUser)
                    .WithMany()
                    .HasForeignKey(p => p.DecidedByUserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    modelBuilder.Entity(entityType.ClrType)
                        .Property(nameof(BaseEntity.Id))
                        .HasDefaultValueSql("gen_random_uuid()");
                }
            }
        }
    }
}
