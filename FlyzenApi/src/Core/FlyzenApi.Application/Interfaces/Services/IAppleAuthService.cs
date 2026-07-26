namespace FlyzenApi.Application.Interfaces.Services
{
    public record AppleUserInfo(string AppleId, string? Email);

    public interface IAppleAuthService
    {
        Task<AppleUserInfo> VerifyAsync(string identityToken);
    }
}
