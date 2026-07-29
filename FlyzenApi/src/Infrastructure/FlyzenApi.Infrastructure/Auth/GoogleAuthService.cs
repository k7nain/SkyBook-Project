using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Auth
{
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly GoogleAuthOptions _options;

        public GoogleAuthService(IOptions<GoogleAuthOptions> options)
        {
            _options = options.Value;
        }

        public async Task<GoogleUserInfo> VerifyAsync(string idToken)
        {
            if (_options.ClientIds.Length == 0)
                throw new BadRequestException("Google sign-in is not configured on this server yet.");

            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = _options.ClientIds,
                });
            }
            catch (InvalidJwtException)
            {
                throw new UnauthorizedAppException("Invalid Google sign-in token.");
            }

            return new GoogleUserInfo(payload.Subject, payload.Email, payload.EmailVerified, payload.GivenName, payload.FamilyName);
        }
    }
}
