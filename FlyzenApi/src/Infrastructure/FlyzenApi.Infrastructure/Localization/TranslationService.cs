using System.Text.Json;
using FlyzenApi.Application.Interfaces.Services;

namespace FlyzenApi.Infrastructure.Localization
{
    // Mirrors the client's src/i18n/index.js `t()`: dotted-key lookup per
    // language, falling back to English, then to the raw key if nothing
    // matches - so a missing translation never throws, it just surfaces
    // visibly (same contract as the frontend).
    public class TranslationService : ITranslationService
    {
        private const string FallbackLanguage = "en";

        private readonly IReadOnlyDictionary<string, JsonElement> _dictionaries;

        public TranslationService()
        {
            _dictionaries = LoadAll();
        }

        public IReadOnlyCollection<string> SupportedLanguages => (IReadOnlyCollection<string>)_dictionaries.Keys;

        public string Translate(string language, string key, IReadOnlyDictionary<string, string>? args = null)
        {
            var value = Lookup(language, key) ?? Lookup(FallbackLanguage, key) ?? key;

            if (args is null || args.Count == 0)
                return value;

            foreach (var (argKey, argValue) in args)
                value = value.Replace($"{{{argKey}}}", argValue);

            return value;
        }

        private string? Lookup(string? language, string key)
        {
            if (string.IsNullOrWhiteSpace(language) || !_dictionaries.TryGetValue(language, out var root))
                return null;

            var current = root;
            foreach (var segment in key.Split('.'))
            {
                if (current.ValueKind != JsonValueKind.Object || !current.TryGetProperty(segment, out var next))
                    return null;
                current = next;
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }

        private static Dictionary<string, JsonElement> LoadAll()
        {
            var assembly = typeof(TranslationService).Assembly;
            var result = new Dictionary<string, JsonElement>();

            foreach (var language in new[] { "az", "en", "ru" })
            {
                var resourceName = $"FlyzenApi.Infrastructure.Localization.Resources.{language}.json";
                using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidOperationException($"Missing embedded translation resource: {resourceName}");

                result[language] = JsonDocument.Parse(stream).RootElement.Clone();
            }

            return result;
        }
    }
}
