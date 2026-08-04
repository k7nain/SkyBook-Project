using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin")]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly IAirlineService _airlineService;
        private readonly ICityService _cityService;
        private readonly ITripCountryService _tripCountryService;
        private readonly ITripCityService _tripCityService;
        private readonly ITripPlaceService _tripPlaceService;
        private readonly IPromoCodeService _promoCodeService;
        private readonly IContentTranslationService _contentTranslationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISeedImageMigrationService _seedImageMigrationService;
        private readonly SeedImageMigrationStatus _seedImageMigrationStatus;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IAdminService adminService,
            IAirlineService airlineService,
            ICityService cityService,
            ITripCountryService tripCountryService,
            ITripCityService tripCityService,
            ITripPlaceService tripPlaceService,
            IPromoCodeService promoCodeService,
            IContentTranslationService contentTranslationService,
            IFileStorageService fileStorageService,
            ISeedImageMigrationService seedImageMigrationService,
            SeedImageMigrationStatus seedImageMigrationStatus,
            IServiceScopeFactory serviceScopeFactory,
            ILogger<AdminController> logger)
        {
            _adminService = adminService;
            _airlineService = airlineService;
            _cityService = cityService;
            _tripCountryService = tripCountryService;
            _tripCityService = tripCityService;
            _tripPlaceService = tripPlaceService;
            _promoCodeService = promoCodeService;
            _contentTranslationService = contentTranslationService;
            _fileStorageService = fileStorageService;
            _seedImageMigrationService = seedImageMigrationService;
            _seedImageMigrationStatus = seedImageMigrationStatus;
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        [HttpGet("bookings")]
        public async Task<ActionResult<IEnumerable<AdminBookingDto>>> GetAllBookings() =>
            Ok(await _adminService.GetAllBookingsAsync());

        [HttpPatch("bookings/{id:guid}/status")]
        public async Task<ActionResult<AdminBookingDto>> UpdateBookingStatus(Guid id, UpdateBookingStatusRequest request) =>
            Ok(await _adminService.UpdateBookingStatusAsync(id, request.Status));

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<AdminUserDto>>> GetAllUsers([FromQuery] string? status) =>
            Ok(await _adminService.GetAllUsersAsync(status));

        [HttpDelete("users/{id:guid}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            await _adminService.DeleteUserAsync(User.GetUserId(), id);
            return NoContent();
        }

        [HttpGet("flights")]
        public async Task<ActionResult<IEnumerable<FlightSummaryDto>>> GetAllFlights() =>
            Ok(await _adminService.GetAllFlightsAsync());

        [HttpPost("flights")]
        public async Task<ActionResult<FlightSummaryDto>> CreateFlight(CreateFlightRequest request) =>
            Ok(await _adminService.CreateFlightAsync(request));

        [HttpPut("flights/{id:guid}/price")]
        public async Task<ActionResult<FlightSummaryDto>> UpdateFlightPrice(Guid id, UpdateFlightPriceRequest request) =>
            Ok(await _adminService.UpdateFlightPriceAsync(id, request));

        [HttpDelete("flights/{id:guid}")]
        public async Task<IActionResult> DeleteFlight(Guid id)
        {
            await _adminService.DeleteFlightAsync(id);
            return NoContent();
        }

        [HttpGet("exchange-rates")]
        public async Task<ActionResult<ExchangeRatesDto>> GetExchangeRates() =>
            Ok(await _adminService.GetExchangeRatesAsync());

        /// <summary>
        /// Aggregate dashboard statistics: revenue (total + by currency + last
        /// 6 months trend), booking counts by status, total verified users, and
        /// total flights. One call for everything an admin dashboard's stat
        /// cards/charts need - suitable for both the mobile and web admin panels.
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<AdminStatisticsDto>> GetStatistics() =>
            Ok(await _adminService.GetStatisticsAsync());

        [HttpPost("airlines")]
        public async Task<ActionResult<AirlineDto>> CreateAirline(CreateAirlineRequest request) =>
            Ok(await _airlineService.CreateAsync(request));

        [HttpPut("cities/{id:guid}")]
        public async Task<ActionResult<CityDto>> UpdateCity(Guid id, UpdateCityRequest request) =>
            Ok(await _cityService.UpdateAsync(id, request));

        [HttpPost("cities/{id:guid}/gallery")]
        public async Task<ActionResult<CityGalleryImageDto>> AddCityGalleryImage(Guid id, AddCityGalleryImageRequest request) =>
            Ok(await _cityService.AddGalleryImageAsync(id, request));

        [HttpPost("trip-countries")]
        public async Task<ActionResult<TripCountryDto>> CreateTripCountry(CreateTripCountryRequest request) =>
            Ok(await _tripCountryService.CreateAsync(request));

        [HttpPut("trip-countries/{id:guid}")]
        public async Task<ActionResult<TripCountryDto>> UpdateTripCountry(Guid id, UpdateTripCountryRequest request) =>
            Ok(await _tripCountryService.UpdateAsync(id, request));

        [HttpDelete("trip-countries/{id:guid}")]
        public async Task<IActionResult> DeleteTripCountry(Guid id)
        {
            await _tripCountryService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("trip-cities")]
        public async Task<ActionResult<TripCityDto>> CreateTripCity(CreateTripCityRequest request) =>
            Ok(await _tripCityService.CreateAsync(request));

        [HttpPut("trip-cities/{id:guid}")]
        public async Task<ActionResult<TripCityDto>> UpdateTripCity(Guid id, UpdateTripCityRequest request) =>
            Ok(await _tripCityService.UpdateAsync(id, request));

        [HttpDelete("trip-cities/{id:guid}")]
        public async Task<IActionResult> DeleteTripCity(Guid id)
        {
            await _tripCityService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("trip-places")]
        public async Task<ActionResult<TripPlaceDto>> CreateTripPlace(CreateTripPlaceRequest request) =>
            Ok(await _tripPlaceService.CreateAsync(request));

        [HttpPut("trip-places/{id:guid}")]
        public async Task<ActionResult<TripPlaceDto>> UpdateTripPlace(Guid id, UpdateTripPlaceRequest request) =>
            Ok(await _tripPlaceService.UpdateAsync(id, request));

        [HttpDelete("trip-places/{id:guid}")]
        public async Task<IActionResult> DeleteTripPlace(Guid id)
        {
            await _tripPlaceService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("trip-places/{id:guid}/gallery")]
        public async Task<ActionResult<TripPlaceImageDto>> AddTripPlaceImage(Guid id, AddTripPlaceImageRequest request) =>
            Ok(await _tripPlaceService.AddImageAsync(id, request));

        [HttpDelete("trip-places/{placeId:guid}/gallery/{imageId:guid}")]
        public async Task<IActionResult> DeleteTripPlaceImage(Guid placeId, Guid imageId)
        {
            await _tripPlaceService.DeleteImageAsync(placeId, imageId);
            return NoContent();
        }

        [HttpPut("trip-places/{id:guid}/gallery/reorder")]
        public async Task<ActionResult<IEnumerable<TripPlaceImageDto>>> ReorderTripPlaceGallery(Guid id, ReorderTripPlaceGalleryRequest request) =>
            Ok(await _tripPlaceService.ReorderGalleryAsync(id, request.ImageIds));

        /// <summary>
        /// Uploads a single image (jpg/png/webp, max 10MB) to local disk storage
        /// and returns its URL for use as a Country cover image, City image, or
        /// a Trip Place gallery entry. Validated server-side regardless of any
        /// client-side checks already performed.
        /// </summary>
        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(11 * 1024 * 1024)]
        public async Task<ActionResult<UploadImageResponse>> UploadImage(IFormFile file, CancellationToken cancellationToken)
        {
            if (file is null)
                throw new BadRequestException("No file was uploaded.");

            await using var stream = file.OpenReadStream();
            var url = await _fileStorageService.SaveImageAsync(stream, file.FileName, file.ContentType, file.Length, cancellationToken);
            return Ok(new UploadImageResponse { Url = url });
        }

        /// <summary>
        /// One-time migration of seeded Dream Trip Country/City/Place images
        /// from external URLs to local file storage. Idempotent - safe to
        /// call again after a partial failure, already-migrated records are
        /// skipped. Can take a couple of minutes for the full seed set.
        /// </summary>
        [HttpPost("migrate-seed-images")]
        public async Task<ActionResult<SeedImageMigrationResult>> MigrateSeedImages(CancellationToken cancellationToken) =>
            Ok(await _seedImageMigrationService.MigrateAsync(cancellationToken));

        [HttpPost("translate")]
        public async Task<ActionResult<ContentTranslateResponse>> Translate(ContentTranslateRequest request, CancellationToken cancellationToken)
        {
            var result = await _contentTranslationService.TranslateAsync(
                request.Text, request.SourceLang, request.TargetLangs, request.Kind, cancellationToken);
            return Ok(new ContentTranslateResponse
            {
                Translations = result.Translations.ToDictionary(kv => kv.Key, kv => kv.Value),
                FailedLangs = result.FailedLangs.ToList(),
            });
        }

        [HttpGet("promo-codes")]
        public async Task<ActionResult<IEnumerable<PromoCodeDto>>> GetAllPromoCodes() =>
            Ok(await _promoCodeService.GetAllAsync());

        [HttpPost("promo-codes")]
        public async Task<ActionResult<PromoCodeDto>> CreatePromoCode(CreatePromoCodeRequest request) =>
            Ok(await _promoCodeService.CreateAsync(request));

        [HttpPut("promo-codes/{id:guid}")]
        public async Task<ActionResult<PromoCodeDto>> UpdatePromoCode(Guid id, UpdatePromoCodeRequest request) =>
            Ok(await _promoCodeService.UpdateAsync(id, request));

        [HttpDelete("promo-codes/{id:guid}")]
        public async Task<IActionResult> DeletePromoCode(Guid id)
        {
            await _promoCodeService.DeleteAsync(id);
            return NoContent();
        }
    }
}
