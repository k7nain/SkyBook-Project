using Microsoft.AspNetCore.Mvc;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        [HttpGet("rates")]
        public IActionResult GetRates()
        {
            var rates = _currencyService.GetExchangeRates();
            return Ok(new { success = true, data = rates });
        }
    }
}
