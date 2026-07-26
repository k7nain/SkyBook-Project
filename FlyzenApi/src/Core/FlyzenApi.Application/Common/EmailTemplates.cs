using System.Net;

namespace FlyzenApi.Application.Common
{
    public static class EmailTemplates
    {
        public static string BuildVerificationEmail(string firstName, string code)
        {
            return Wrap(
                title: "Verify your email",
                bodyHtml: $@"
                    <p>Hi {WebUtility.HtmlEncode(firstName)},</p>
                    <p>Thanks for signing up for SkyBook. Use the code below to verify your email address:</p>
                    <div style=""text-align:center;margin:32px 0;"">
                        <span style=""display:inline-block;font-size:32px;letter-spacing:8px;font-weight:700;color:#1E3A8A;background:#EFF6FF;border-radius:12px;padding:16px 24px;"">{code}</span>
                    </div>
                    <p>This code expires in 10 minutes. If you didn't create a SkyBook account, you can safely ignore this email.</p>");
        }

        public static string BuildPasswordResetEmail(string firstName, string resetLink)
        {
            return Wrap(
                title: "Reset your password",
                bodyHtml: $@"
                    <p>Hi {WebUtility.HtmlEncode(firstName)},</p>
                    <p>We received a request to reset your SkyBook password. Tap the button below to choose a new one:</p>
                    <div style=""text-align:center;margin:32px 0;"">
                        <a href=""{resetLink}"" style=""display:inline-block;background:#3B82F6;color:#fff;text-decoration:none;font-weight:700;padding:14px 28px;border-radius:12px;"">Reset password</a>
                    </div>
                    <p>This link expires in 20 minutes. If you didn't request a password reset, you can safely ignore this email.</p>");
        }

        public static string BuildBookingConfirmationEmail(string firstName, string pnr, string route, string departureTime, string seatNumbers, string totalPrice)
        {
            return Wrap(
                title: "Your booking is confirmed",
                bodyHtml: $@"
                    <p>Hi {WebUtility.HtmlEncode(firstName)},</p>
                    <p>Your SkyBook reservation is confirmed. Here are the details:</p>
                    <div style=""background:#EFF6FF;border-radius:12px;padding:20px;margin:24px 0;"">
                        <p style=""margin:4px 0;""><b>PNR:</b> {WebUtility.HtmlEncode(pnr)}</p>
                        <p style=""margin:4px 0;""><b>Route:</b> {WebUtility.HtmlEncode(route)}</p>
                        <p style=""margin:4px 0;""><b>Departure:</b> {WebUtility.HtmlEncode(departureTime)}</p>
                        <p style=""margin:4px 0;""><b>Seat(s):</b> {WebUtility.HtmlEncode(seatNumbers)}</p>
                        <p style=""margin:4px 0;""><b>Total paid:</b> {WebUtility.HtmlEncode(totalPrice)}</p>
                    </div>
                    <p>Have a great trip!</p>");
        }

        public static string BuildPriceChangeEmail(string firstName, string route, string oldPrice, string newPrice)
        {
            return Wrap(
                title: "Price update for your pending booking",
                bodyHtml: $@"
                    <p>Hi {WebUtility.HtmlEncode(firstName)},</p>
                    <p>The price for your pending flight ({WebUtility.HtmlEncode(route)}) has changed:</p>
                    <div style=""text-align:center;margin:24px 0;"">
                        <span style=""text-decoration:line-through;color:#94A3B8;font-size:16px;"">{WebUtility.HtmlEncode(oldPrice)}</span>
                        <span style=""font-size:22px;font-weight:700;color:#1E3A8A;margin-left:12px;"">{WebUtility.HtmlEncode(newPrice)}</span>
                    </div>
                    <p>No action is needed unless you'd like to review your booking.</p>");
        }

        public static string BuildTripReminderEmail(string firstName, string route, string departureTime, int daysBefore)
        {
            var when = daysBefore == 1 ? "tomorrow" : $"in {daysBefore} days";
            return Wrap(
                title: "Your trip is coming up",
                bodyHtml: $@"
                    <p>Hi {WebUtility.HtmlEncode(firstName)},</p>
                    <p>Just a reminder that your flight ({WebUtility.HtmlEncode(route)}) departs {when}, on {WebUtility.HtmlEncode(departureTime)}.</p>
                    <p>Safe travels!</p>");
        }

        private static string Wrap(string title, string bodyHtml)
        {
            return $@"
                <div style=""font-family:Segoe UI,Helvetica,Arial,sans-serif;background:#F1F5F9;padding:32px 0;"">
                    <div style=""max-width:480px;margin:0 auto;background:#ffffff;border-radius:24px;overflow:hidden;"">
                        <div style=""background:linear-gradient(135deg,#1E3A8A,#3B82F6);padding:24px 32px;"">
                            <span style=""color:#fff;font-size:22px;font-weight:800;letter-spacing:1px;"">SkyBook</span>
                        </div>
                        <div style=""padding:32px;color:#0F172A;font-size:15px;line-height:1.6;"">
                            <h2 style=""margin-top:0;color:#0F172A;"">{WebUtility.HtmlEncode(title)}</h2>
                            {bodyHtml}
                            <p style=""color:#64748B;font-size:13px;margin-top:32px;"">— The SkyBook team</p>
                        </div>
                    </div>
                </div>";
        }
    }
}
