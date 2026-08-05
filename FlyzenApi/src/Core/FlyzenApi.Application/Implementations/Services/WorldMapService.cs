using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.Application.Implementations.Services
{
    public class WorldMapService : IWorldMapService
    {
        // Server-side cap per viewport query, tiered by zoom - a world-view bounding
        // box can span the whole globe, so it's capped harder than a city-level view.
        // Real visual clustering (pins merging into a count bubble) is left to the
        // client map library; this cap just keeps a single response payload light.
        private const int WorldViewLimit = 150;
        private const int RegionViewLimit = 400;
        private const int CityViewLimit = 1000;

        private readonly ITripPlaceRepository _tripPlaceRepository;
        private readonly ICityRepository _cityRepository;

        public WorldMapService(ITripPlaceRepository tripPlaceRepository, ICityRepository cityRepository)
        {
            _tripPlaceRepository = tripPlaceRepository;
            _cityRepository = cityRepository;
        }

        public async Task<IEnumerable<MapMarkerDto>> GetMarkersInBoundsAsync(MapBoundsRequest request)
        {
            var language = NormalizeLanguage(request.Lang);
            var limit = request.ZoomLevel switch
            {
                <= 3 => WorldViewLimit,
                <= 6 => RegionViewLimit,
                _ => CityViewLimit,
            };

            var (west, east) = NormalizeLongitudeBounds(request.WestLng, request.EastLng);

            var places = await _tripPlaceRepository.GetMarkersInBoundsAsync(
                request.SouthLat, request.NorthLat, west, east, limit);

            return places.Select(p => p.ToMarkerDto(language));
        }

        public async Task<DestinationDetailDto> GetDestinationDetailAsync(Guid id, string? lang)
        {
            var place = await _tripPlaceRepository.GetDetailByIdAsync(id)
                ?? throw new NotFoundException("Destination not found.");

            var dto = place.ToDetailDto(NormalizeLanguage(lang));

            // Best-effort link to the bookable City used by flight search (see
            // DestinationDetailDto.BookableCityId) - matched on the canonical
            // English name, not the localized display name, since Cities.Name
            // is always English. Null NameEn (not yet translated) or no
            // matching bookable city both correctly leave this null.
            if (!string.IsNullOrWhiteSpace(place.City.NameEn))
            {
                var bookableCity = await _cityRepository.GetByNameAsync(place.City.NameEn);
                dto.BookableCityId = bookableCity?.Id;
            }

            return dto;
        }

        public Task<IEnumerable<MapMarkerDto>> GetUserRecommendedMarkersAsync(Guid userId, string? lang)
        {
            // F1's Dream Trip AI recommendation is a stateless, on-demand AI call -
            // it never persists a result or a numeric score, so there is nothing to
            // look up for a given userId yet. Returns empty so the map's
            // "recommended" layer renders its empty state rather than fake data;
            // wiring this up for real requires F1 to persist quiz results first.
            return Task.FromResult(Enumerable.Empty<MapMarkerDto>());
        }

        private static string NormalizeLanguage(string? lang) => lang?.ToLowerInvariant() switch
        {
            "en" => "en",
            "ru" => "ru",
            _ => "az",
        };

        // A raw Leaflet viewport isn't validated to +/-180 at the DTO level (see
        // MapBoundsRequest) because it can legitimately fall outside that range:
        // worldCopyJump doesn't clamp getBounds(), so a low-zoom view can span
        // more than 360 degrees (the whole world, possibly more than once), and
        // repeated panning offsets both edges by whole multiples of 360 with no
        // upper bound (observed +/-945 in practice, not just a small
        // antimeridian overshoot). This wraps both edges into a single
        // [-180, 180) window that TripPlaceRepository's existing
        // "west <= east ? AND : OR-wraparound" query logic already expects.
        private static (double West, double East) NormalizeLongitudeBounds(double west, double east)
        {
            if (double.IsNaN(west) || double.IsNaN(east) || double.IsInfinity(west) || double.IsInfinity(east))
            {
                throw new BadRequestException("Longitude bounds must be finite numbers.");
            }

            // The whole world (or more) is in view - every longitude is visible
            // somewhere, so there's nothing meaningful left to restrict.
            if (east - west >= 360)
            {
                return (-180, 180);
            }

            return (NormalizeLongitude(west), NormalizeLongitude(east));
        }

        private static double NormalizeLongitude(double lng)
        {
            var wrapped = lng % 360; // C#'s % keeps the sign of the dividend, magnitude < 360.
            if (wrapped < -180) wrapped += 360;
            else if (wrapped >= 180) wrapped -= 360;
            return wrapped;
        }
    }
}
