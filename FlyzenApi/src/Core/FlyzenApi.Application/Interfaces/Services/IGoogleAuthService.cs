namespace FlyzenApi.Application.Interfaces.Services
{
    public record GoogleUserInfo(string GoogleId, string Email, bool EmailVerified, string? FirstName, string? LastName);

    public interface IGoogleAuthService
    {
        Task<GoogleUserInfo> VerifyAsync(string idToken);
    }
}
