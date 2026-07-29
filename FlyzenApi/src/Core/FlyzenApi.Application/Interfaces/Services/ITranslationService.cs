namespace FlyzenApi.Application.Interfaces.Services
{
    // Backend equivalent of the client's src/i18n/index.js `t()` - dotted-key
    // lookup with English fallback, plus {placeholder} interpolation. Used to
    // render notification titles/bodies and transactional emails in the
    // recipient's User.LanguagePreference rather than a fixed server language.
    public interface ITranslationService
    {
        IReadOnlyCollection<string> SupportedLanguages { get; }

        string Translate(string language, string key, IReadOnlyDictionary<string, string>? args = null);
    }
}
