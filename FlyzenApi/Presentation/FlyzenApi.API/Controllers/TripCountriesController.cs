using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/trip-countries")]
    public class TripCountriesController : ControllerBase
    {
        private readonly ITripCountryService _tripCountryService;

        public TripCountriesController(ITripCountryService tripCountryService)
        {
            _tripCountryService = tripCountryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TripCountryDto>>> GetAll() =>
            Ok(await _tripCountryService.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TripCountryDto>> GetById(Guid id)
        {
            var country = await _tripCountryService.GetByIdAsync(id);
            return country is null ? NotFound() : Ok(country);
        }
    }
}
