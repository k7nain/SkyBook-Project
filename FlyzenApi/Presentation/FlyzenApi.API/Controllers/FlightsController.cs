using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/flights")]
    public class FlightsController : ControllerBase
    {
        private readonly IFlightService _flightService;
        private readonly ILogger<FlightsController> _logger;

        public FlightsController(IFlightService flightService, ILogger<FlightsController> logger)
        {
            _flightService = flightService;
            _logger = logger;
        }

        // fromCityId/departureDate omitted => "any origin" search (e.g. a World
        // Map marker click, which has no from-city or date the user picked yet) -
        // see FlightService.SearchAsync/FlightRepository.SearchAsync.
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FlightSummaryDto>>> Search(
            [FromQuery] Guid? fromCityId,
            [FromQuery] Guid toCityId,
            [FromQuery] DateTime? departureDate,
            [FromQuery] int passengers = 1)
        {
            var results = await _flightService.SearchAsync(fromCityId, toCityId, departureDate, passengers);

            // Best-effort: awaited (not truly fire-and-forget, which would race the
            // scoped DbContext getting disposed once this request completes), but a
            // logging failure must never fail a real search response. No-ops for
            // guests (User.Identity is unauthenticated - this endpoint has no [Authorize]).
            if (User.Identity?.IsAuthenticated == true)
            {
                try
                {
                    await _flightService.LogSearchAsync(User.GetUserId(), fromCityId, toCityId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to log flight search for personalization.");
                }
            }

            return Ok(results);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FlightDetailDto>> GetById(Guid id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            return flight is null ? NotFound() : Ok(flight);
        }
    }
}
