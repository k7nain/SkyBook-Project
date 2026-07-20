using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using FlyzenApi.Application.Features.Cities.Queries.GetAllCities;
using FlyzenApi.Application.Features.Cities.Queries.GetCityById;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCities()
        {
            var result = await _mediator.Send(new GetAllCitiesQuery());
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(Guid id)
        {
            var result = await _mediator.Send(new GetCityByIdQuery { Id = id });
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }

        [HttpGet("{id}/gallery")]
        public async Task<IActionResult> GetCityGallery(Guid id)
        {
            var result = await _mediator.Send(new GetCityByIdQuery { Id = id });
            if (!result.Success)
                return NotFound(result.Message);

            return Ok(result);
        }
    }
}