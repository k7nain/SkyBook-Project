using MediatR;
using FlyzenApi.Application.Common.Models;

namespace FlyzenApi.Application.Features.Auth.Queries.Login
{
    public class LoginQuery : IRequest<Result<string>>
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}