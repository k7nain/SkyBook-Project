using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class CatalogController : ControllerBase
    {
        private readonly ICatalogService _catalogService;

        public CatalogController(ICatalogService catalogService)
        {
            _catalogService = catalogService;
        }

        [HttpGet("meals")]
        public async Task<ActionResult<IEnumerable<MealOptionDto>>> GetMeals() =>
            Ok(await _catalogService.GetMealOptionsAsync());

        [HttpGet("baggage-options")]
        public async Task<ActionResult<IEnumerable<BaggageOptionDto>>> GetBaggageOptions() =>
            Ok(await _catalogService.GetBaggageOptionsAsync());
    }
}
