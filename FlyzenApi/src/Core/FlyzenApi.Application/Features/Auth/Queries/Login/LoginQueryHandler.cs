using System.Threading;
using System.Threading.Tasks;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Application.Interfaces;
using FlyzenApi.Domain.Repositories;
using MediatR;

namespace FlyzenApi.Application.Features.Auth.Queries.Login
{
    public class LoginQueryHandler : IRequestHandler<LoginQuery, Result<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public LoginQueryHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<Result<string>> Handle(LoginQuery request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
                return Result<string>.FailureResult("Invalid email or password.");

            if (!_authService.VerifyPassword(request.Password, user.PasswordHash))
                return Result<string>.FailureResult("Invalid email or password.");

            var token = _authService.GenerateToken(user.Id, user.Email);

            return Result<string>.SuccessResult(token, "Login successful");
        }
    }
}