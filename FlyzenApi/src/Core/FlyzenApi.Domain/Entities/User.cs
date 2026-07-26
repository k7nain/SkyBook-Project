using System;
using System.Collections.Generic;
using FlyzenApi.Domain.Entities.Common;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Domain.Entities
{
    public class User : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string CurrencyPreference { get; set; } = "AZN";
        public string LanguagePreference { get; set; } = "en";
        public UserRole Role { get; set; } = UserRole.User;

        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailVerificationCodeHash { get; set; }
        public DateTime? EmailVerificationExpiresAt { get; set; }
        public DateTime? EmailVerificationLastSentAt { get; set; }

        public string? PasswordResetTokenHash { get; set; }
        public DateTime? PasswordResetExpiresAt { get; set; }
        public DateTime? PasswordResetLastSentAt { get; set; }

        public int TokenVersion { get; set; } = 0;

        public string? GoogleId { get; set; }
        public string? AppleId { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}