using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/map")]
    public class WorldMapController : ControllerBase
    {
        private readonly IWorldMapService _worldMapService;

        public WorldMapController(IWorldMapService worldMapService)
        {
            _worldMapService = worldMapService;
        }

        [HttpGet("markers")]
        [EnableRateLimiting("world-map-markers")]
        public async Task<ActionResult<IEnumerable<MapMarkerDto>>> GetMarkers([FromQuery] MapBoundsRequest request) =>
            Ok(await _worldMapService.GetMarkersInBoundsAsync(request));

        [HttpGet("destinations/{id:guid}")]
        public async Task<ActionResult<DestinationDetailDto>> GetDestination(Guid id, [FromQuery] string? lang) =>
            Ok(await _worldMapService.GetDestinationDetailAsync(id, lang));

        [HttpGet("recommended")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MapMarkerDto>>> GetRecommended([FromQuery] string? lang) =>
            Ok(await _worldMapService.GetUserRecommendedMarkersAsync(User.GetUserId(), lang));
    }
}
