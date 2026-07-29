using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/airlines")]
    public class AirlinesController : ControllerBase
    {
        private readonly IAirlineService _airlineService;

        public AirlinesController(IAirlineService airlineService)
        {
            _airlineService = airlineService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirlineDto>>> GetAll() =>
            Ok(await _airlineService.GetAllAsync());
    }
}
