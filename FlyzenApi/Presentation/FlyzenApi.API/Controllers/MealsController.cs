using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using FlyzenApi.Domain.Repositories;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MealsController : ControllerBase
    {
        private readonly IFlightRepository _flightRepository;

        public MealsController(IFlightRepository flightRepository)
        {
            _flightRepository = flightRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMeals()
        {
            var meals = await _flightRepository.GetAllMealOptionsAsync();
            return Ok(new { success = true, data = meals });
        }
    }
}
