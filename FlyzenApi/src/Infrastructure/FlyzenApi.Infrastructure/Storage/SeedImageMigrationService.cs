using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Persistence.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlyzenApi.Infrastructure.Storage
{
    // One-time bulk migration: seeded Country/City/Place records point at
    // external Wikimedia URLs (scripts/seedDreamTrips); this re-hosts them
    // through IFileStorageService so the app stops depending on an external
    // host staying up. Reads AppDbContext directly rather than through the
    // usual repository interfaces - this is bulk data tooling that touches
    // every row across three tables at once, not a domain operation, and
    // adding GetAllAsync-style methods to every repository just for a
    // one-time job isn't worth the permanent surface area.
    //
    // Sequential, not concurrent, with real 429 handling: Wikimedia's edge
    // rate-limits by request volume over a rolling window - confirmed by
    // testing plain curl (not just this service) against the same URLs,
    // where even a slow, isolated sequence gets an intermittent 429 here and
    // there. It's not a hard block and it's not client-specific: retrying
    // the individual item after a real pause gets through. What doesn't
    // work is treating a 429 as "unlucky, move on" at a fast pace - that
    // keeps the rolling window full and the block never clears. The
    // consecutive-failure cooldown below is the actual fix: back off the
    // whole batch, not just the one item, when the pattern suggests we're
    // currently inside the throttle window rather than just unlucky once.
    public class SeedImageMigrationService : ISeedImageMigrationService
    {
        private const string LocalPrefix = "/uploads/";
        private const int MaxRetries = 6;
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(4);
        private static readonly TimeSpan PauseBetweenRequests = TimeSpan.FromSeconds(1.5);
        private static readonly TimeSpan CooldownAfterConsecutiveFailures = TimeSpan.FromSeconds(25);
        private const int ConsecutiveFailuresBeforeCooldown = 3;
        private const int SaveEveryNItems = 20;

        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<SeedImageMigrationService> _logger;
        private readonly SeedImageMigrationStatus _status;

        public SeedImageMigrationService(AppDbContext context, IFileStorageService fileStorageService, HttpClient httpClient, ILogger<SeedImageMigrationService> logger, SeedImageMigrationStatus status)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _status = status;
            _httpClient = httpClient;
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            // Wikimedia's CDN 403s any request with no User-Agent (or a generic
            // one) under its bot-protection policy - HttpClient sends none by
            // default, unlike curl/browsers.
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SkyBook-ImageMigration/1.0 (one-time seed image migration script)");
            _logger = logger;
        }

        public async Task<SeedImageMigrationResult> MigrateAsync(CancellationToken cancellationToken = default)
        {
            var result = new SeedImageMigrationResult();
            var candidates = new List<MigrationCandidate>();

            var countries = await _context.TripCountries.ToListAsync(cancellationToken);
            foreach (var country in countries)
            {
                if (IsExternal(country.CoverImage))
                    candidates.Add(new MigrationCandidate("Country", country.Id, country.NameAz, country.CoverImage!, url => country.CoverImage = url));
                else if (!string.IsNullOrEmpty(country.CoverImage))
                    result.AlreadyMigrated++;
            }

            var cities = await _context.TripCities.ToListAsync(cancellationToken);
            foreach (var city in cities)
            {
                if (IsExternal(city.Image))
                    candidates.Add(new MigrationCandidate("City", city.Id, city.NameAz, city.Image!, url => city.Image = url));
                else if (!string.IsNullOrEmpty(city.Image))
                    result.AlreadyMigrated++;
            }

            var places = await _context.TripPlaces.Include(p => p.Images).ToListAsync(cancellationToken);
            foreach (var place in places)
            {
                foreach (var image in place.Images)
                {
                    if (IsExternal(image.ImageUrl))
                        candidates.Add(new MigrationCandidate("Place", place.Id, place.NameAz, image.ImageUrl, url => image.ImageUrl = url));
                    else
                        result.AlreadyMigrated++;
                }
            }

            result.TotalCandidates = candidates.Count;
            _status.TotalCandidates = candidates.Count;
            _status.AlreadyMigrated = result.AlreadyMigrated;

            var sinceLastSave = 0;
            var consecutiveFailures = 0;
            for (var i = 0; i < candidates.Count; i++)
            {
                var candidate = candidates[i];
                var outcome = await DownloadAndSaveAsync(candidate, cancellationToken);

                if (outcome.Success)
                {
                    candidate.Apply(outcome.NewUrl!);
                    result.Migrated++;
                    consecutiveFailures = 0;
                }
                else
                {
                    var failure = new SeedImageMigrationFailure
                    {
                        EntityType = candidate.EntityType,
                        EntityId = candidate.EntityId,
                        Name = candidate.Name,
                        Url = candidate.Url,
                        Error = outcome.Error!,
                    };
                    result.Failed++;
                    result.Failures.Add(failure);
                    _status.Failures.Add(failure);
                    consecutiveFailures++;
                }

                _status.Processed = i + 1;
                _status.Migrated = result.Migrated;
                _status.Failed = result.Failed;

                // Save periodically, not just at the end - if this long-running
                // request is ever interrupted (client disconnect, server
                // restart), everything migrated up to the last checkpoint is
                // already durable, and IsExternal()'s skip check makes a
                // re-run pick up exactly where this one left off.
                if (++sinceLastSave >= SaveEveryNItems || i == candidates.Count - 1)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    sinceLastSave = 0;
                }

                if (i == candidates.Count - 1)
                    break;

                if (consecutiveFailures >= ConsecutiveFailuresBeforeCooldown)
                {
                    _logger.LogWarning("{Count} consecutive image downloads failed - cooling down for {Seconds}s before continuing.", consecutiveFailures, CooldownAfterConsecutiveFailures.TotalSeconds);
                    await Task.Delay(CooldownAfterConsecutiveFailures, cancellationToken);
                    consecutiveFailures = 0;
                }
                else
                {
                    await Task.Delay(PauseBetweenRequests, cancellationToken);
                }
            }

            return result;
        }

        private async Task<DownloadOutcome> DownloadAndSaveAsync(MigrationCandidate candidate, CancellationToken cancellationToken)
        {
            try
            {
                for (var attempt = 1; ; attempt++)
                {
                    using var response = await _httpClient.GetAsync(candidate.Url, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if ((int)response.StatusCode == 429 && attempt <= MaxRetries)
                    {
                        var delay = response.Headers.RetryAfter?.Delta ?? RetryDelay;
                        await Task.Delay(delay, cancellationToken);
                        continue;
                    }

                    if (!response.IsSuccessStatusCode)
                        return new DownloadOutcome(false, null, $"HTTP {(int)response.StatusCode}");

                    var contentType = response.Content.Headers.ContentType?.MediaType ?? "";
                    var extension = ExtensionFor(contentType);
                    if (extension is null)
                        return new DownloadOutcome(false, null, $"Unsupported content type: {contentType}");

                    var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    using var stream = new MemoryStream(bytes);
                    var newUrl = await _fileStorageService.SaveMigratedImageAsync(stream, "seed" + extension, contentType, bytes.LongLength, cancellationToken);
                    return new DownloadOutcome(true, newUrl, null);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Seed image migration failed for {EntityType} {EntityId} ({Name}): {Url}", candidate.EntityType, candidate.EntityId, candidate.Name, candidate.Url);
                return new DownloadOutcome(false, null, ex.Message);
            }
        }

        private static bool IsExternal(string? url) =>
            !string.IsNullOrEmpty(url) && !url.StartsWith(LocalPrefix, StringComparison.OrdinalIgnoreCase);

        private static string? ExtensionFor(string contentType) => contentType.ToLowerInvariant() switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => null,
        };

        private record MigrationCandidate(string EntityType, Guid EntityId, string Name, string Url, Action<string> Apply);

        private record DownloadOutcome(bool Success, string? NewUrl, string? Error);
    }
}
