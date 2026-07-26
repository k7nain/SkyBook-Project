using FlyzenApi.Application.Common;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
using FlyzenApi.Domain.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace FlyzenApi.Application.Implementations.Services
{
    public class AuthService : IAuthService
    {
        private static readonly TimeSpan VerificationCodeLifetime = TimeSpan.FromMinutes(10);
        private static readonly TimeSpan ResendCooldown = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan PasswordResetLifetime = TimeSpan.FromMinutes(20);

        private readonly IUserRepository _userRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IAppleAuthService _appleAuthService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository,
            IBookingRepository bookingRepository,
            ITokenService tokenService,
            IPasswordHasher<User> passwordHasher,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IGoogleAuthService googleAuthService,
            IAppleAuthService appleAuthService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _bookingRepository = bookingRepository;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _googleAuthService = googleAuthService;
            _appleAuthService = appleAuthService;
            _logger = logger;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing is not null)
                throw new ConflictException("A user with this email already exists.");

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Role = UserRole.User,
                IsEmailConfirmed = false,
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            await _userRepository.AddAsync(user);
            await SendVerificationCodeAsync(user);

            return new RegisterResponse
            {
                Email = user.Email,
                Message = "Registration successful. Please check your email for a verification code.",
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email)
                ?? throw new UnauthorizedAppException("Invalid email or password.");

            if (user.PasswordHash is null)
                throw new UnauthorizedAppException("This account uses Google or Apple sign-in. Please continue with that instead.");

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
            if (result == PasswordVerificationResult.Failed)
                throw new UnauthorizedAppException("Invalid email or password.");

            if (!user.IsEmailConfirmed)
                throw new UnauthorizedAppException("Please verify your email before logging in.");

            return new AuthResponse { Token = _tokenService.GenerateToken(user), User = user.ToDto() };
        }

        public async Task<AuthResponse> VerifyEmailAsync(VerifyEmailRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email)
                ?? throw new BadRequestException("Invalid or expired verification code.");

            if (user.IsEmailConfirmed)
                throw new BadRequestException("This email is already verified.");

            var codeHash = TokenGenerator.Hash(request.Code);
            if (user.EmailVerificationCodeHash != codeHash
                || user.EmailVerificationExpiresAt is null
                || user.EmailVerificationExpiresAt < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired verification code.");

            user.IsEmailConfirmed = true;
            user.EmailVerificationCodeHash = null;
            user.EmailVerificationExpiresAt = null;

            await _userRepository.UpdateAsync(user);

            return new AuthResponse { Token = _tokenService.GenerateToken(user), User = user.ToDto() };
        }

        public async Task<MessageResponse> ResendVerificationAsync(ResendVerificationRequest request)
        {
            const string genericMessage = "If an account with that email exists and isn't verified yet, we've sent a new code.";

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is not null && !user.IsEmailConfirmed)
            {
                if (user.EmailVerificationLastSentAt is not null
                    && DateTime.UtcNow - user.EmailVerificationLastSentAt < ResendCooldown)
                    throw new TooManyRequestsException("Please wait a moment before requesting another code.");

                await SendVerificationCodeAsync(user);
            }

            return new MessageResponse { Message = genericMessage };
        }

        public async Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, string appBaseUrl)
        {
            const string genericMessage = "If an account with that email exists, we've sent a password reset link.";

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user is not null && user.PasswordHash is not null)
            {
                var withinCooldown = user.PasswordResetLastSentAt is not null
                    && DateTime.UtcNow - user.PasswordResetLastSentAt < ResendCooldown;

                if (!withinCooldown)
                {
                    var rawToken = TokenGenerator.GenerateSecureToken();
                    user.PasswordResetTokenHash = TokenGenerator.Hash(rawToken);
                    user.PasswordResetExpiresAt = DateTime.UtcNow.Add(PasswordResetLifetime);
                    user.PasswordResetLastSentAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);

                    var resetLink = $"{appBaseUrl}/reset-password?token={Uri.EscapeDataString(rawToken)}";
                    await TrySendEmailAsync(
                        user.Email,
                        "Reset your SkyBook password",
                        EmailTemplates.BuildPasswordResetEmail(user.FirstName, resetLink));
                }
            }

            return new MessageResponse { Message = genericMessage };
        }

        public async Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var tokenHash = TokenGenerator.Hash(request.Token);
            var user = await _userRepository.GetByPasswordResetTokenHashAsync(tokenHash);

            if (user is null
                || user.PasswordResetExpiresAt is null
                || user.PasswordResetExpiresAt < DateTime.UtcNow)
                throw new BadRequestException("Invalid or expired reset link.");

            user.PasswordHash = _passwordHasher.HashPassword(user, request.NewPassword);
            user.PasswordResetTokenHash = null;
            user.PasswordResetExpiresAt = null;
            user.PasswordResetLastSentAt = null;
            user.TokenVersion++;

            await _userRepository.UpdateAsync(user);

            return new MessageResponse { Message = "Your password has been reset. Please log in with your new password." };
        }

        public async Task<AuthResponse> GoogleLoginAsync(GoogleLoginRequest request)
        {
            var googleUser = await _googleAuthService.VerifyAsync(request.IdToken);
            if (!googleUser.EmailVerified)
                throw new BadRequestException("Your Google account's email isn't verified.");

            var user = await LoginOrLinkProviderAsync(
                findByProviderId: () => _userRepository.GetByGoogleIdAsync(googleUser.GoogleId),
                linkProviderId: u => u.GoogleId = googleUser.GoogleId,
                email: googleUser.Email,
                firstName: googleUser.FirstName ?? "Google",
                lastName: googleUser.LastName ?? "User");

            return new AuthResponse { Token = _tokenService.GenerateToken(user), User = user.ToDto() };
        }

        public async Task<AuthResponse> AppleLoginAsync(AppleLoginRequest request)
        {
            var appleUser = await _appleAuthService.VerifyAsync(request.IdentityToken);

            var existingByAppleId = await _userRepository.GetByAppleIdAsync(appleUser.AppleId);
            var email = appleUser.Email ?? request.Email;

            if (existingByAppleId is null && email is null)
                throw new BadRequestException("Could not complete Apple sign-in. Please try again.");

            User user;
            if (existingByAppleId is not null)
            {
                user = existingByAppleId;
            }
            else
            {
                user = await LoginOrLinkProviderAsync(
                    findByProviderId: () => Task.FromResult<User?>(null),
                    linkProviderId: u => u.AppleId = appleUser.AppleId,
                    email: email!,
                    firstName: request.FirstName ?? "Apple",
                    lastName: request.LastName ?? "User");
            }

            return new AuthResponse { Token = _tokenService.GenerateToken(user), User = user.ToDto() };
        }

        private async Task<User> LoginOrLinkProviderAsync(
            Func<Task<User?>> findByProviderId,
            Action<User> linkProviderId,
            string email,
            string firstName,
            string lastName)
        {
            var existingByProviderId = await findByProviderId();
            if (existingByProviderId is not null)
                return existingByProviderId;

            var existingByEmail = await _userRepository.GetByEmailAsync(email);
            if (existingByEmail is not null)
            {
                linkProviderId(existingByEmail);
                existingByEmail.IsEmailConfirmed = true;
                await _userRepository.UpdateAsync(existingByEmail);
                return existingByEmail;
            }

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = string.Empty,
                Role = UserRole.User,
                IsEmailConfirmed = true,
                PasswordHash = null,
            };
            linkProviderId(user);

            await _userRepository.AddAsync(user);
            return user;
        }

        private async Task SendVerificationCodeAsync(User user)
        {
            var code = TokenGenerator.GenerateOtp();
            user.EmailVerificationCodeHash = TokenGenerator.Hash(code);
            user.EmailVerificationExpiresAt = DateTime.UtcNow.Add(VerificationCodeLifetime);
            user.EmailVerificationLastSentAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            await TrySendEmailAsync(
                user.Email,
                "Verify your SkyBook email",
                EmailTemplates.BuildVerificationEmail(user.FirstName, code));
        }

        private async Task TrySendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                await _emailService.SendAsync(toEmail, subject, htmlBody);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send email to {Email} with subject '{Subject}'.", toEmail, subject);
            }
        }

        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");
            return user.ToDto();
        }

        public async Task<UserDto> UpdateProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found.");

            if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _userRepository.GetByEmailAsync(request.Email);
                if (existing is not null && existing.Id != userId)
                    throw new ConflictException("A user with this email already exists.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.PhoneNumber = request.PhoneNumber;
            if (request.ProfilePictureUrl is not null)
                user.ProfilePictureUrl = request.ProfilePictureUrl;

            await _userRepository.UpdateAsync(user);
            return user.ToDto();
        }

        public async Task DeleteAccountAsync(Guid userId)
        {
            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var user = await _userRepository.GetByIdAsync(userId)
                    ?? throw new NotFoundException("User not found.");

                var bookings = await _bookingRepository.GetByUserIdAsync(userId);
                foreach (var booking in bookings)
                {
                    if (booking.Status != BookingStatus.Cancelled)
                    {
                        foreach (var passenger in booking.Passengers)
                        {
                            if (passenger.Seat is not null)
                                passenger.Seat.IsAvailable = true;
                        }
                    }
                    await _bookingRepository.DeleteAsync(booking);
                }

                await _userRepository.DeleteAsync(user);
                return true;
            });
        }
    }
}
