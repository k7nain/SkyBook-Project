using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/baggage-options")]
    public class BaggageController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;

        public BaggageController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBaggageOptions()
        {
            var baggage = await _flightRepository.GetAllBaggageOptionsAsync();
            return Ok(new { success = true, data = baggage });
        }
    }
}
