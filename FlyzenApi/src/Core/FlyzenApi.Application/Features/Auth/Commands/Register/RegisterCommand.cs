using MediatR;
using FlyzenApi.Application.Common.Models;

namespace FlyzenApi.Application.Features.Auth.Commands.Register
{
    public class RegisterCommand : IRequest<Result<string>>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
    }
}