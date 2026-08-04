using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class ContactRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Subject { get; set; }

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        // Honeypot: real users never see or fill this field (hidden on the
        // form). A bot that auto-fills every input will populate it, so a
        // non-empty value here means "silently drop, pretend success" -
        // never send an email for it (see ContactService).
        public string? Website { get; set; }
    }
}
