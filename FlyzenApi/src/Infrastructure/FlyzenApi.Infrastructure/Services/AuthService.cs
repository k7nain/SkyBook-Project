using System.Security.Cryptography;
using System.Text;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IJwtTokenService _jwtTokenService;

        public AuthService(IJwtTokenService jwtTokenService)
        {
            _jwtTokenService = jwtTokenService;
        }

        public string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(hash);
        }

        public string GenerateToken(Guid userId, string email)
        {
            return _jwtTokenService.GenerateToken(userId, email);
        }
    }
}
