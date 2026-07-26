using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
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

        public AdminController(
            IAdminService adminService,
            IAirlineService airlineService,
            ICityService cityService,
            ITripCountryService tripCountryService,
            ITripCityService tripCityService,
            ITripPlaceService tripPlaceService,
            IPromoCodeService promoCodeService)
        {
            _adminService = adminService;
            _airlineService = airlineService;
            _cityService = cityService;
            _tripCountryService = tripCountryService;
            _tripCityService = tripCityService;
            _tripPlaceService = tripPlaceService;
            _promoCodeService = promoCodeService;
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
