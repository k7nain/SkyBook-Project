using System.Net;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Email
{
    public class ContactService : IContactService
    {
        private readonly IEmailService _emailService;
        private readonly ContactOptions _options;

        public ContactService(IEmailService emailService, IOptions<ContactOptions> options)
        {
            _emailService = emailService;
            _options = options.Value;
        }

        public async Task SubmitAsync(ContactRequest request)
        {
            // Honeypot tripped - a real visitor never fills this hidden field.
            // Return silently (no exception, no "rejected" signal) so a bot
            // gets no feedback that anything was different from a real submit.
            if (!string.IsNullOrWhiteSpace(request.Website))
                return;

            var subject = string.IsNullOrWhiteSpace(request.Subject)
                ? $"Yeni əlaqə mesajı - {request.Name}"
                : $"Əlaqə formu: {request.Subject}";

            // User-supplied values are interpolated into an HTML email body,
            // so they must be encoded - SmtpEmailService sends htmlBody as-is
            // with no escaping of its own.
            var name = WebUtility.HtmlEncode(request.Name);
            var email = WebUtility.HtmlEncode(request.Email);
            var message = WebUtility.HtmlEncode(request.Message).Replace("\n", "<br/>");
            var subjectLine = string.IsNullOrWhiteSpace(request.Subject)
                ? ""
                : $"<p><strong>Mövzu:</strong> {WebUtility.HtmlEncode(request.Subject)}</p>";

            var htmlBody = $@"
                <h2>Yeni əlaqə formu mesajı</h2>
                <p><strong>Ad:</strong> {name}</p>
                <p><strong>Email:</strong> {email}</p>
                {subjectLine}
                <p><strong>Mesaj:</strong></p>
                <p>{message}</p>";

            await _emailService.SendAsync(_options.RecipientEmail, subject, htmlBody);
        }
    }
}
