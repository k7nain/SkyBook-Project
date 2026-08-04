using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/dream-trip")]
    [Authorize]
    public class DreamTripAiController : ControllerBase
    {
        private readonly IDreamTripAiService _dreamTripAiService;

        public DreamTripAiController(IDreamTripAiService dreamTripAiService)
        {
            _dreamTripAiService = dreamTripAiService;
        }

        [HttpPost("recommend")]
        [EnableRateLimiting("dream-trip-ai")]
        public async Task<ActionResult<DreamTripRecommendResponse>> Recommend(DreamTripRecommendRequest request, CancellationToken cancellationToken)
        {
            var response = await _dreamTripAiService.RecommendAsync(User.GetUserId(), request, cancellationToken);
            return Ok(response);
        }
    }
}
