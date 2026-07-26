using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace FlyzenApi.Infrastructure.Email
{
    public class SmtpEmailService : IEmailService
    {
        private readonly SmtpOptions _options;

        public SmtpEmailService(IOptions<SmtpOptions> options)
        {
            _options = options.Value;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
                throw new BadRequestException("Email delivery is not configured on this server yet. Set the Smtp:Host/User/Password settings to enable it.");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

            using var client = new SmtpClient();

            try
            {
                var secureSocketOptions = _options.Port == 465
                    ? SecureSocketOptions.SslOnConnect
                    : _options.EnableSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

                await client.ConnectAsync(_options.Host, _options.Port, secureSocketOptions);
                await client.AuthenticateAsync(_options.User, _options.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                throw new BadRequestException($"Could not send the email: {ex.Message}");
            }
        }
    }
}
