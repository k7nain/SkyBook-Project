using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    /// <summary>
    /// One-time migration of seeded Dream Trip Country/City/Place image URLs
    /// (external Wikimedia links from scripts/seedDreamTrips) to local file
    /// storage. Idempotent - already-migrated (/uploads/...) fields are
    /// skipped, so it's safe to re-run after a partial failure.
    /// </summary>
    public interface ISeedImageMigrationService
    {
        Task<SeedImageMigrationResult> MigrateAsync(CancellationToken cancellationToken = default);
    }
}
