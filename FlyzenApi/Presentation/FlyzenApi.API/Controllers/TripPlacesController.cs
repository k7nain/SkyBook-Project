using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/trip-places")]
    public class TripPlacesController : ControllerBase
    {
        private readonly ITripPlaceService _tripPlaceService;

        public TripPlacesController(ITripPlaceService tripPlaceService)
        {
            _tripPlaceService = tripPlaceService;
        }

        [HttpGet("by-city/{cityId:guid}")]
        public async Task<ActionResult<IEnumerable<TripPlaceDto>>> GetByCity(Guid cityId) =>
            Ok(await _tripPlaceService.GetByCityIdAsync(cityId));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TripPlaceDto>> GetById(Guid id)
        {
            var place = await _tripPlaceService.GetByIdAsync(id);
            return place is null ? NotFound() : Ok(place);
        }

        [HttpGet("{id:guid}/gallery")]
        public async Task<ActionResult<IEnumerable<TripPlaceImageDto>>> GetGallery(Guid id) =>
            Ok(await _tripPlaceService.GetGalleryAsync(id));
    }
}
