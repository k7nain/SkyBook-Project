using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        private readonly ICityService _cityService;

        public CitiesController(ICityService cityService)
        {
            _cityService = cityService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityDto>>> GetAll() =>
            Ok(await _cityService.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CityDto>> GetById(Guid id)
        {
            var city = await _cityService.GetByIdAsync(id);
            return city is null ? NotFound() : Ok(city);
        }

        [HttpGet("{id:guid}/gallery")]
        public async Task<ActionResult<IEnumerable<CityGalleryImageDto>>> GetGallery(Guid id) =>
            Ok(await _cityService.GetGalleryAsync(id));
    }
}
