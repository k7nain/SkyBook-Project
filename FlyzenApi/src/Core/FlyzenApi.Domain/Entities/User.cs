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
        public string PasswordHash { get; set; } = string.Empty;
        public string? ProfilePictureUrl { get; set; }
        public string CurrencyPreference { get; set; } = "USD";
        public string LanguagePreference { get; set; } = "en";

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}