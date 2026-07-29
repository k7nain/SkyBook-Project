using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    // Any logged-in user (not just Admins) needs live rates - this backs the
    // user-facing currency picker (Settings > Currency), which previously read
    // a hardcoded/never-refreshed rate table and is the reason this controller
    // exists separately from AdminController's own exchange-rates endpoint.
    [ApiController]
    [Route("api/currency")]
    [Authorize]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyConversionService _currencyConversionService;

        public CurrencyController(ICurrencyConversionService currencyConversionService)
        {
            _currencyConversionService = currencyConversionService;
        }

        [HttpGet("rates")]
        public async Task<ActionResult<ExchangeRatesDto>> GetRates()
        {
            var rates = await _currencyConversionService.GetRatesAsync();
            return Ok(new ExchangeRatesDto
            {
                RatesToAzn = rates.RatesToAzn.ToDictionary(r => r.Key, r => r.Value),
                FetchedAtUtc = rates.FetchedAtUtc,
                Source = rates.Source,
            });
        }
    }
}
