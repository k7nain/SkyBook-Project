using System.Threading.Tasks;

namespace FlyzenApi.Application.Interfaces
{
    public interface IAuthService
    {
        Task<string> GenerateTokenAsync(Domain.Entities.User user);
    }
}