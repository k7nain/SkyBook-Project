using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FlyzenApi.Infrastructure.Auth
{
    public class AppleAuthService : IAppleAuthService
    {
        private const string Issuer = "https://appleid.apple.com";
        private const string JwksUrl = "https://appleid.apple.com/auth/keys";
        private static readonly TimeSpan JwksCacheLifetime = TimeSpan.FromHours(6);
        private static readonly SemaphoreSlim JwksLock = new(1, 1);
        private static JsonWebKeySet? _cachedJwks;
        private static DateTime _cachedAt;

        private readonly HttpClient _httpClient;
        private readonly AppleAuthOptions _options;

        public AppleAuthService(HttpClient httpClient, IOptions<AppleAuthOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<AppleUserInfo> VerifyAsync(string identityToken)
        {
            if (_options.ClientIds.Length == 0)
                throw new BadRequestException("Apple sign-in is not configured on this server yet.");

            var jwks = await GetJwksAsync();
            var handler = new JwtSecurityTokenHandler();

            ClaimsPrincipal principal;
            try
            {
                principal = handler.ValidateToken(identityToken, new TokenValidationParameters
                {
                    ValidIssuer = Issuer,
                    ValidAudiences = _options.ClientIds,
                    IssuerSigningKeys = jwks.Keys,
                    ValidateLifetime = true,
                }, out _);
            }
            catch (SecurityTokenException)
            {
                throw new UnauthorizedAppException("Invalid Apple sign-in token.");
            }

            var subject = principal.FindFirst("sub")?.Value
                ?? throw new UnauthorizedAppException("Invalid Apple sign-in token.");
            var email = principal.FindFirst("email")?.Value;

            return new AppleUserInfo(subject, email);
        }

        private async Task<JsonWebKeySet> GetJwksAsync()
        {
            if (_cachedJwks is not null && DateTime.UtcNow - _cachedAt < JwksCacheLifetime)
                return _cachedJwks;

            await JwksLock.WaitAsync();
            try
            {
                if (_cachedJwks is not null && DateTime.UtcNow - _cachedAt < JwksCacheLifetime)
                    return _cachedJwks;

                var json = await _httpClient.GetStringAsync(JwksUrl);
                _cachedJwks = new JsonWebKeySet(json);
                _cachedAt = DateTime.UtcNow;
                return _cachedJwks;
            }
            finally
            {
                JwksLock.Release();
            }
        }
    }
}
