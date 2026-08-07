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

        /// <summary>
        /// Passive, ongoing recommendations from the user's own behavior (past
        /// searches, booked/journaled destinations) - the "Sizin ucun" Home page
        /// section. Cached per-user for ~24h server-side (see
        /// UserRecommendationCache), so this is safe to call on every Home load
        /// without re-hitting OpenRouter each time - deliberately NOT behind the
        /// same "dream-trip-ai" rate limiter as the quiz endpoint, since a cache
        /// hit does no AI call at all.
        /// </summary>
        [HttpGet("for-you")]
        public async Task<ActionResult<DreamTripRecommendResponse>> ForYou(CancellationToken cancellationToken)
        {
            var response = await _dreamTripAiService.RecommendForYouAsync(User.GetUserId(), cancellationToken);
            return Ok(response);
        }
    }
}
