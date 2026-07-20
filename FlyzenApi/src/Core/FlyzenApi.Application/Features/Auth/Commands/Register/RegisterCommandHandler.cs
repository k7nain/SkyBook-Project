using System;
using System.Threading;
using System.Threading.Tasks;
using FlyzenApi.Application.Common.Models;
using FlyzenApi.Application.Interfaces;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using MediatR;

namespace FlyzenApi.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<string>>
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public RegisterCommandHandler(IUserRepository userRepository, IAuthService authService)
        {
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<Result<string>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var existingUser = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
                return Result<string>.FailureResult("User already exists.");

            var user = new User
            {
                Id = Guid.NewGuid(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                PasswordHash = _authService.HashPassword(request.Password)
            };

            await _userRepository.AddAsync(user);

            return Result<string>.SuccessResult(user.Id.ToString(), "User registered successfully");
        }
    }
}}