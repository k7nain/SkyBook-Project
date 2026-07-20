using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System;
using FlyzenApi.Application.Features.Auth.Commands.Register;
using FlyzenApi.Application.Features.Auth.Queries.Login;
using FlyzenApi.Domain.Repositories;
using System.Security.Claims;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUserRepository _userRepository;

        public AuthController(IMediator mediator, IUserRepository userRepository)
        {
            _mediator = mediator;
            _userRepository = userRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.Success)
                return Unauthorized(result.Message);

            return Ok(result);
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var parsedUserId))
                return Unauthorized(new { message = "User not authenticated" });

            var user = await _userRepository.GetByIdAsync(parsedUserId);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new { success = true, data = user });
        }
    }
}