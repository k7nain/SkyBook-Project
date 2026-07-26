using FlyzenApi.Domain.Entities;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface ITokenService
    {
        string GenerateToken(User user);
    }
}
