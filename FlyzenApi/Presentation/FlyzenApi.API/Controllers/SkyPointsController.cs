using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/sky-points")]
    [Authorize]
    public class SkyPointsController : ControllerBase
    {
        private readonly ISkyPointsService _skyPointsService;

        public SkyPointsController(ISkyPointsService skyPointsService)
        {
            _skyPointsService = skyPointsService;
        }

        // The current balance itself doesn't need its own endpoint - it rides
        // along on UserDto (see /api/auth/me and login/register responses).
        [HttpGet("history")]
        public async Task<ActionResult<IEnumerable<SkyPointsTransactionDto>>> GetHistory() =>
            Ok(await _skyPointsService.GetHistoryAsync(User.GetUserId()));
    }
}
