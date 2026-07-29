using System.Net;
using FlyzenApi.Application.Interfaces.Services;

namespace FlyzenApi.Application.Common
{
    // All text comes from ITranslationService (the "email.*" namespace in
    // Localization/Resources/{az,en,ru}.json), rendered in the recipient's
    // language at send time - callers pass `translator` + `language`
    // (typically recipient.LanguagePreference) instead of literal strings.
    public static class EmailTemplates
    {
        public static string BuildVerificationEmail(ITranslationService translator, string language, string firstName, string code)
        {
            var greeting = translator.Translate(language, "email.greeting", Args(("name", WebUtility.HtmlEncode(firstName))));
            return Wrap(translator, language,
                title: translator.Translate(language, "email.verification.title"),
                bodyHtml: $@"
                    <p>{greeting}</p>
                    <p>{translator.Translate(language, "email.verification.intro")}</p>
                    <div style=""text-align:center;margin:32px 0;"">
                        <span style=""display:inline-block;font-size:32px;letter-spacing:8px;font-weight:700;color:#1E3A8A;background:#EFF6FF;border-radius:12px;padding:16px 24px;"">{code}</span>
                    </div>
                    <p>{translator.Translate(language, "email.verification.expiry")}</p>");
        }

        public static string BuildPasswordResetEmail(ITranslationService translator, string language, string firstName, string resetLink)
        {
            var greeting = translator.Translate(language, "email.greeting", Args(("name", WebUtility.HtmlEncode(firstName))));
            var button = translator.Translate(language, "email.passwordReset.button");
            return Wrap(translator, language,
                title: translator.Translate(language, "email.passwordReset.title"),
                bodyHtml: $@"
                    <p>{greeting}</p>
                    <p>{translator.Translate(language, "email.passwordReset.intro")}</p>
                    <div style=""text-align:center;margin:32px 0;"">
                        <a href=""{resetLink}"" style=""display:inline-block;background:#3B82F6;color:#fff;text-decoration:none;font-weight:700;padding:14px 28px;border-radius:12px;"">{button}</a>
                    </div>
                    <p>{translator.Translate(language, "email.passwordReset.expiry")}</p>");
        }

        public static string BuildBookingConfirmationEmail(
            ITranslationService translator, string language,
            string firstName, string pnr, string route, string departureTime, string seatNumbers, string totalPrice)
        {
            var greeting = translator.Translate(language, "email.greeting", Args(("name", WebUtility.HtmlEncode(firstName))));
            return Wrap(translator, language,
                title: translator.Translate(language, "email.bookingConfirmed.title"),
                bodyHtml: $@"
                    <p>{greeting}</p>
                    <p>{translator.Translate(language, "email.bookingConfirmed.intro")}</p>
                    <div style=""background:#EFF6FF;border-radius:12px;padding:20px;margin:24px 0;"">
                        <p style=""margin:4px 0;""><b>{translator.Translate(language, "email.bookingConfirmed.pnrLabel")}</b> {WebUtility.HtmlEncode(pnr)}</p>
                        <p style=""margin:4px 0;""><b>{translator.Translate(language, "email.bookingConfirmed.routeLabel")}</b> {WebUtility.HtmlEncode(route)}</p>
                        <p style=""margin:4px 0;""><b>{translator.Translate(language, "email.bookingConfirmed.departureLabel")}</b> {WebUtility.HtmlEncode(departureTime)}</p>
                        <p style=""margin:4px 0;""><b>{translator.Translate(language, "email.bookingConfirmed.seatsLabel")}</b> {WebUtility.HtmlEncode(seatNumbers)}</p>
                        <p style=""margin:4px 0;""><b>{translator.Translate(language, "email.bookingConfirmed.totalLabel")}</b> {WebUtility.HtmlEncode(totalPrice)}</p>
                    </div>
                    <p>{translator.Translate(language, "email.bookingConfirmed.closing")}</p>");
        }

        public static string BuildPriceChangeEmail(ITranslationService translator, string language, string firstName, string route, string oldPrice, string newPrice)
        {
            var greeting = translator.Translate(language, "email.greeting", Args(("name", WebUtility.HtmlEncode(firstName))));
            var intro = translator.Translate(language, "email.priceChange.intro", Args(("route", WebUtility.HtmlEncode(route))));
            return Wrap(translator, language,
                title: translator.Translate(language, "email.priceChange.title"),
                bodyHtml: $@"
                    <p>{greeting}</p>
                    <p>{intro}</p>
                    <div style=""text-align:center;margin:24px 0;"">
                        <span style=""text-decoration:line-through;color:#94A3B8;font-size:16px;"">{WebUtility.HtmlEncode(oldPrice)}</span>
                        <span style=""font-size:22px;font-weight:700;color:#1E3A8A;margin-left:12px;"">{WebUtility.HtmlEncode(newPrice)}</span>
                    </div>
                    <p>{translator.Translate(language, "email.priceChange.closing")}</p>");
        }

        public static string BuildTripReminderEmail(ITranslationService translator, string language, string firstName, string route, string departureTime, int daysBefore)
        {
            var greeting = translator.Translate(language, "email.greeting", Args(("name", WebUtility.HtmlEncode(firstName))));
            var bodyKey = daysBefore == 1 ? "email.tripReminder.bodyTomorrow" : "email.tripReminder.bodyDays";
            var body = translator.Translate(language, bodyKey, Args(
                ("route", WebUtility.HtmlEncode(route)),
                ("departureTime", WebUtility.HtmlEncode(departureTime)),
                ("days", daysBefore.ToString())));

            return Wrap(translator, language,
                title: translator.Translate(language, "email.tripReminder.title"),
                bodyHtml: $@"
                    <p>{greeting}</p>
                    <p>{body}</p>
                    <p>{translator.Translate(language, "email.tripReminder.closing")}</p>");
        }

        private static string Wrap(ITranslationService translator, string language, string title, string bodyHtml)
        {
            var footer = translator.Translate(language, "email.footer");
            return $@"
                <div style=""font-family:Segoe UI,Helvetica,Arial,sans-serif;background:#F1F5F9;padding:32px 0;"">
                    <div style=""max-width:480px;margin:0 auto;background:#ffffff;border-radius:24px;overflow:hidden;"">
                        <div style=""background:linear-gradient(135deg,#1E3A8A,#3B82F6);padding:24px 32px;"">
                            <span style=""color:#fff;font-size:22px;font-weight:800;letter-spacing:1px;"">SkyBook</span>
                        </div>
                        <div style=""padding:32px;color:#0F172A;font-size:15px;line-height:1.6;"">
                            <h2 style=""margin-top:0;color:#0F172A;"">{WebUtility.HtmlEncode(title)}</h2>
                            {bodyHtml}
                            <p style=""color:#64748B;font-size:13px;margin-top:32px;"">{WebUtility.HtmlEncode(footer)}</p>
                        </div>
                    </div>
                </div>";
        }

        private static Dictionary<string, string> Args(params (string Key, string Value)[] pairs) =>
            pairs.ToDictionary(p => p.Key, p => p.Value);
    }
}
