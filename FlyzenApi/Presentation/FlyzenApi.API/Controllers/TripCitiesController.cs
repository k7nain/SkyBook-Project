using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/trip-cities")]
    public class TripCitiesController : ControllerBase
    {
        private readonly ITripCityService _tripCityService;

        public TripCitiesController(ITripCityService tripCityService)
        {
            _tripCityService = tripCityService;
        }

        [HttpGet("by-country/{countryId:guid}")]
        public async Task<ActionResult<IEnumerable<TripCityDto>>> GetByCountry(Guid countryId) =>
            Ok(await _tripCityService.GetByCountryIdAsync(countryId));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TripCityDto>> GetById(Guid id)
        {
            var city = await _tripCityService.GetByIdAsync(id);
            return city is null ? NotFound() : Ok(city);
        }
    }
}
