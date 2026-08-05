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

        public FlightsController(IFlightService flightService)
        {
            _flightService = flightService;
        }

        // fromCityId/departureDate omitted => "any origin" search (e.g. a World
        // Map marker click, which has no from-city or date the user picked yet) -
        // see FlightService.SearchAsync/FlightRepository.SearchAsync.
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<FlightSummaryDto>>> Search(
            [FromQuery] Guid? fromCityId,
            [FromQuery] Guid toCityId,
            [FromQuery] DateTime? departureDate,
            [FromQuery] int passengers = 1) =>
            Ok(await _flightService.SearchAsync(fromCityId, toCityId, departureDate, passengers));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FlightDetailDto>> GetById(Guid id)
        {
            var flight = await _flightService.GetByIdAsync(id);
            return flight is null ? NotFound() : Ok(flight);
        }
    }
}
