using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request);
        Task<MessageResponse> ResendVerificationAsync(ResendVerificationRequest request);
        Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string appBaseUrl);
        Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request);
        Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request);
        Task<AuthResponse> AppleLoginAsync(AppleLoginRequest request);
        Task<UserDto> GetProfileAsync(Guid userId);
        Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request);
        Task DeleteAccountAsync(Guid userId);
    }
}
