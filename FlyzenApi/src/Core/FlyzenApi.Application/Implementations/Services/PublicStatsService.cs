using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace FlyzenApi.Application.Implementations.Services
{
    // Registered as a Singleton (see Program.cs) so its cache survives across
    // requests, mirroring CbarCurrencyConversionService's shape - but unlike
    // that service, this one needs Scoped repositories (backed by DbContext),
    // so it can't just take them as constructor dependencies the normal way.
    // It takes IServiceScopeFactory instead and only opens a short-lived scope
    // on a cache-miss refresh; every cache-hit read touches no scope/DbContext
    // at all. Exact real-time precision doesn't matter for a marketing stats
    // display, so a few minutes of staleness is the deliberate trade-off for
    // not re-running these counts on every single About-page load.
    public class PublicStatsService : IPublicStatsService
    {
        private const int CacheMinutes = 10;

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly SemaphoreSlim _refreshLock = new(1, 1);
        private PublicStatsDto? _cached;
        private DateTime _cachedAtUtc;

        public PublicStatsService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<PublicStatsDto> GetStatsAsync(CancellationToken cancellationToken = default)
        {
            if (IsFresh())
                return _cached!;

            await _refreshLock.WaitAsync(cancellationToken);
            try
            {
                // Another request may have refreshed the cache while we were waiting.
                if (IsFresh())
                    return _cached!;

                using var scope = _scopeFactory.CreateScope();
                var flightRepository = scope.ServiceProvider.GetRequiredService<IFlightRepository>();
                var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                _cached = new PublicStatsDto
                {
                    Destinations = await flightRepository.CountDistinctDestinationCitiesAsync(),
                    HappyCustomers = await userRepository.CountAsync(isEmailConfirmed: true),
                    CompletedFlights = await flightRepository.CountByStatusAsync(FlightStatus.Completed),
                };
                _cachedAtUtc = DateTime.UtcNow;
                return _cached;
            }
            finally
            {
                _refreshLock.Release();
            }
        }

        private bool IsFresh() =>
            _cached is not null && DateTime.UtcNow - _cachedAtUtc < TimeSpan.FromMinutes(CacheMinutes);
    }
}
