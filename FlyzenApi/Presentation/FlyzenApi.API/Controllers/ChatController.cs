using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/chat")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost]
        [EnableRateLimiting("chat")]
        public async Task<ActionResult<ChatResponse>> Send(ChatRequest request)
        {
            var reply = await _chatService.GetReplyAsync(request.Message, request.History);
            return Ok(new ChatResponse { Reply = reply });
        }
    }
}
