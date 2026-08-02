namespace FlyzenApi.Application.Interfaces.Services
{
    public record ContentTranslateResult(IReadOnlyDictionary<string, string> Translations, IReadOnlyList<string> FailedLangs);

    // AI-assisted translation for admin-authored content (Dream Trip
    // country/city/place AZ/EN/RU fields), via an external LLM (OpenRouter).
    // Unrelated to ITranslationService, which renders this app's own static
    // i18n strings (notification/email templates) - this one generates new
    // text from admin input instead of looking up a fixed key.
    // Never throws on a bad/slow AI response - languages that fail come back
    // in FailedLangs so the caller can fall back to manual entry instead of
    // blocking the whole save.
    public interface IContentTranslationService
    {
        Task<ContentTranslateResult> TranslateAsync(
            string text,
            string sourceLang,
            IReadOnlyList<string> targetLangs,
            string kind,
            CancellationToken cancellationToken = default);
    }
}
