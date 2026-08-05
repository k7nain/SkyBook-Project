using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    // Deliberately public/unauthenticated (no [Authorize] anywhere in this
    // controller) - for the marketing "About Us" page, which anonymous
    // visitors see before logging in. Every action here must only ever return
    // aggregate counts, never anything user-identifiable.
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly IPublicStatsService _publicStatsService;

        public PublicController(IPublicStatsService publicStatsService)
        {
            _publicStatsService = publicStatsService;
        }

        [HttpGet("statistics")]
        public async Task<ActionResult<PublicStatsDto>> GetStatistics() =>
            Ok(await _publicStatsService.GetStatsAsync());
    }
}
