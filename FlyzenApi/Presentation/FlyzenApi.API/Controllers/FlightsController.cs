using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using FlyzenApi.Application.Features.Flights.Queries.SearchFlights;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IFlightRepository _flightRepository;

        public FlightsController(IMediator mediator, IFlightRepository flightRepository)
        {
            _mediator = mediator;
            _flightRepository = flightRepository;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] Guid from, [FromQuery] Guid to, [FromQuery] DateTime departureDate, [FromQuery] int passengers)
        {
            var query = new SearchFlightsQuery
            {
                FromCityId = from,
                ToCityId = to,
                DepartureDate = departureDate,
                Passengers = passengers
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFlightById(Guid id)
        {
            var flight = await _flightRepository.GetByIdAsync(id);
            if (flight == null)
                return NotFound(new { message = "Flight not found" });

            return Ok(new { success = true, data = flight });
        }

        [HttpGet("{flightId}/seats")]
        public async Task<IActionResult> GetFlightSeats(Guid flightId)
        {
            var flight = await _flightRepository.GetByIdAsync(flightId);
            if (flight == null)
                return NotFound(new { message = "Flight not found" });

            return Ok(new { success = true, data = flight.Seats });
        }
    }
}