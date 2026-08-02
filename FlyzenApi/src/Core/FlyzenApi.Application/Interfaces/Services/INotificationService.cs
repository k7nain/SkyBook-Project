using FlyzenApi.Application.DTOs;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationDto>> GetMineAsync(Guid userId);
        Task MarkAsReadAsync(Guid id, Guid userId);
        Task DeleteAsync(Guid id, Guid userId);
        Task DeleteAllAsync(Guid userId);
        Task MarkAllAsReadAsync(Guid userId);

        // Called from other services (booking creation, admin flight-price updates,
        // the trip-reminder/flight-notification background jobs) - not exposed via
        // any controller directly. The caller passes the full recipient User (it
        // already has it loaded in every call site) rather than have this service
        // re-fetch it - needed here for both Email and LanguagePreference.
        //
        // titleKey/bodyKey are dotted paths into the shared notifications.* i18n
        // namespace (same keys the client's src/i18n files use), args are the
        // {placeholder} substitutions for both. The row's Title/Message are
        // rendered once, now, in recipient.LanguagePreference, as a fallback
        // snapshot; TitleKey/BodyKey/args are stored too so the app can
        // dynamically re-render in whatever language the user currently has
        // selected (see ITranslationService).
        //
        // emailSubject/emailHtmlBody must already be fully rendered by the
        // caller (via ITranslationService + EmailTemplates) in the recipient's
        // language - an email can't be re-rendered after it's sent.
        Task CreateAsync(
            User recipient,
            string titleKey,
            string bodyKey,
            IReadOnlyDictionary<string, string>? args,
            NotificationType type,
            Guid? bookingId = null,
            string? emailSubject = null,
            string? emailHtmlBody = null);
    }
}
