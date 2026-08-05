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
        // ISO 639-1-ish app language code: "az" | "en" | "ru" (see src/i18n on the
        // client). Drives which language backend-generated notifications/emails
        // render in - see ITranslationService.
        public string LanguagePreference { get; set; } = "az";
        // IANA time zone name (e.g. "Asia/Baku"). Null means "not set" - the
        // client falls back to the device's local time zone in that case.
        public string? PreferredTimezone { get; set; }
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

        // Denormalized running total (not summed from SkyPointsTransactions on
        // every read) for fast display in the nav bar/profile - always kept in
        // sync with a transaction row in the same SaveChanges, see
        // BookingService's earn/reverse/redeem logic.
        public int SkyPointsBalance { get; set; } = 0;

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}