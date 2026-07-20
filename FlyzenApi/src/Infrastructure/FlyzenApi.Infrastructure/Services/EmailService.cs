using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using FlyzenApi.Application.Interfaces;

namespace FlyzenApi.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachmentData = null, string? attachmentFileName = null)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var host = smtpSettings["Host"] ?? throw new InvalidOperationException("SMTP Host not configured");
                var port = int.Parse(smtpSettings["Port"] ?? "587");
                var user = smtpSettings["User"] ?? throw new InvalidOperationException("SMTP User not configured");
                var password = smtpSettings["Password"] ?? throw new InvalidOperationException("SMTP Password not configured");

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("FlyzenAPI", user));
                message.To.Add(new MailboxAddress(string.Empty, toEmail));
                message.Subject = subject;

                var bodyBuilder = new BodyBuilder { HtmlBody = body };

                if (attachmentData != null && !string.IsNullOrEmpty(attachmentFileName))
                {
                    bodyBuilder.Attachments.Add(attachmentFileName, attachmentData, ContentType.Parse("application/pdf"));
                }

                message.Body = bodyBuilder.ToMessageBody();

                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(user, password);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
