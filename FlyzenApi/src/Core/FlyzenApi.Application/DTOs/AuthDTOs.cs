using System.ComponentModel.DataAnnotations;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.DTOs
{
    public class RegisterRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // The app's currently-selected UI language at signup time ("az"|"en"|"ru"),
        // so the very first email (verification code) renders correctly.
        // Defaults to "az" server-side if omitted.
        public string? Language { get; set; }
    }

    public class LoginRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, MaxLength(30)]
        public string PhoneNumber { get; set; } = string.Empty;

        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateTimezoneRequest
    {
        // Null clears the preference, meaning "use the device's local time zone".
        [MaxLength(64)]
        public string? PreferredTimezone { get; set; }
    }

    public class UpdateLanguageRequest
    {
        // "az" | "en" | "ru" - validated against ITranslationService.SupportedLanguages.
        [Required]
        public string Language { get; set; } = string.Empty;
    }

    public class VerifyEmailRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6), MaxLength(6)]
        public string Code { get; set; } = string.Empty;
    }

    public class ResendVerificationRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordRequest
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;
    }

    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }

    public class AppleLoginRequest
    {
        [Required]
        public string IdentityToken { get; set; } = string.Empty;

        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class RegisterResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class MessageResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDto User { get; set; } = null!;
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string CurrencyPreference { get; set; } = "USD";
        public string LanguagePreference { get; set; } = "en";
        public string? PreferredTimezone { get; set; }
        public UserRole Role { get; set; }
        public int SkyPointsBalance { get; set; }
    }
}
