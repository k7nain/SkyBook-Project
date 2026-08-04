namespace FlyzenApi.Application.DTOs
{
    public class SeedImageMigrationFailure
    {
        public string EntityType { get; set; } = string.Empty; // "Country" | "City" | "Place"
        public Guid EntityId { get; set; }
        public string Name { get; set; } = string.Empty; // NameAz, to identify the record for a manual fix
        public string Url { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }

    public class SeedImageMigrationResult
    {
        public int TotalCandidates { get; set; }
        public int AlreadyMigrated { get; set; }
        public int Migrated { get; set; }
        public int Failed { get; set; }
        public List<SeedImageMigrationFailure> Failures { get; set; } = new();
    }

    /// <summary>
    /// Shared, singleton, in-memory progress tracker for the migration -
    /// runs detached from the triggering HTTP request (it takes many
    /// minutes, too long to hold one request open reliably), so progress is
    /// polled via GET rather than read off the POST response.
    /// </summary>
    public class SeedImageMigrationStatus
    {
        public bool IsRunning { get; set; }
        public bool HasRun { get; set; }
        public DateTime? StartedAtUtc { get; set; }
        public DateTime? FinishedAtUtc { get; set; }
        public int TotalCandidates { get; set; }
        public int AlreadyMigrated { get; set; }
        public int Processed { get; set; }
        public int Migrated { get; set; }
        public int Failed { get; set; }
        public List<SeedImageMigrationFailure> Failures { get; set; } = new();
        public string? CrashError { get; set; }
    }
}
