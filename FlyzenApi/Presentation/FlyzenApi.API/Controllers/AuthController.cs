using FlyzenApi.API.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlyzenApi.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request) =>
            Ok(await _authService.RegisterAsync(request));

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request) =>
            Ok(await _authService.LoginAsync(request));

        [HttpPost("verify-email")]
        public async Task<ActionResult<AuthResponse>> VerifyEmail(VerifyEmailRequest request) =>
            Ok(await _authService.VerifyEmailAsync(request));

        [HttpPost("resend-verification")]
        public async Task<ActionResult<MessageResponse>> ResendVerification(ResendVerificationRequest request) =>
            Ok(await _authService.ResendVerificationAsync(request));

        [HttpPost("forgot-password")]
        public async Task<ActionResult<MessageResponse>> ForgotPassword(ForgotPasswordRequest request)
        {
            var appBaseUrl = $"{Request.Scheme}://{Request.Host}";
            return Ok(await _authService.ForgotPasswordAsync(request, appBaseUrl));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<MessageResponse>> ResetPassword(ResetPasswordRequest request) =>
            Ok(await _authService.ResetPasswordAsync(request));

        [HttpPost("google")]
        public async Task<ActionResult<AuthResponse>> GoogleLogin(GoogleLoginRequest request) =>
            Ok(await _authService.GoogleLoginAsync(request));

        [HttpPost("apple")]
        public async Task<ActionResult<AuthResponse>> AppleLogin(AppleLoginRequest request) =>
            Ok(await _authService.AppleLoginAsync(request));

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> Me() =>
            Ok(await _authService.GetProfileAsync(User.GetUserId()));

        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateMe(UpdateProfileRequest request) =>
            Ok(await _authService.UpdateProfileAsync(User.GetUserId(), request));

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMe()
        {
            await _authService.DeleteAccountAsync(User.GetUserId());
            return NoContent();
        }
    }
}
