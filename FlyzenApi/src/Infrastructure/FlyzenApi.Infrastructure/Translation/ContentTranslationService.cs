using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlyzenApi.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Translation
{
    // Calls OpenRouter's OpenAI-compatible chat-completions endpoint, asking for
    // a single JSON-mode response that covers every requested target language in
    // one call (cheaper/faster than one request per language). Any failure -
    // missing API key, network error, timeout, malformed/incomplete JSON back -
    // is caught here and turned into FailedLangs entries; this service never
    // throws for those cases, since a translation hiccup shouldn't block an
    // admin from saving a Country/City/Place with manually-typed text instead.
    public class ContentTranslationService : IContentTranslationService
    {
        private static readonly IReadOnlyDictionary<string, string> LanguageNames = new Dictionary<string, string>
        {
            ["az"] = "Azerbaijani",
            ["en"] = "English",
            ["ru"] = "Russian",
        };

        private readonly HttpClient _httpClient;
        private readonly ContentTranslationOptions _options;
        private readonly ILogger<ContentTranslationService> _logger;

        public ContentTranslationService(HttpClient httpClient, IOptions<ContentTranslationOptions> options, ILogger<ContentTranslationService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ContentTranslateResult> TranslateAsync(
            string text,
            string sourceLang,
            IReadOnlyList<string> targetLangs,
            string kind,
            CancellationToken cancellationToken = default)
        {
            var translations = new Dictionary<string, string>();
            var failed = new List<string>();

            var source = sourceLang.Trim().ToLowerInvariant();
            var validTargets = targetLangs
                .Select(l => l.Trim().ToLowerInvariant())
                .Where(l => l != source)
                .Distinct()
                .ToList();

            var unknown = validTargets.Where(l => !LanguageNames.ContainsKey(l)).ToList();
            foreach (var lang in unknown)
            {
                failed.Add(lang);
                validTargets.Remove(lang);
            }
            if (!LanguageNames.ContainsKey(source) || validTargets.Count == 0)
            {
                failed.AddRange(validTargets);
                return new ContentTranslateResult(translations, failed);
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("Content translation requested but OpenRouter:ApiKey is not configured.");
                failed.AddRange(validTargets);
                return new ContentTranslateResult(translations, failed);
            }

            try
            {
                var content = await CallOpenRouterAsync(text, source, validTargets, kind, cancellationToken);
                foreach (var lang in validTargets)
                {
                    if (content.TryGetValue(lang, out var value) && !string.IsNullOrWhiteSpace(value))
                        translations[lang] = value.Trim();
                    else
                        failed.Add(lang);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "OpenRouter translation call failed for {Kind} text from {Source}.", kind, source);
                failed.AddRange(validTargets);
            }

            return new ContentTranslateResult(translations, failed);
        }

        private async Task<Dictionary<string, string>> CallOpenRouterAsync(
            string text, string sourceLang, IReadOnlyList<string> targetLangs, string kind, CancellationToken cancellationToken)
        {
            var systemPrompt = BuildSystemPrompt(sourceLang, targetLangs, kind);

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = text },
                },
                response_format = new { type = "json_object" },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
            {
                Content = JsonContent.Create(requestBody),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(20));

            using var response = await _httpClient.SendAsync(request, cts.Token);
            var body = await response.Content.ReadAsStringAsync(cts.Token);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"OpenRouter returned {(int)response.StatusCode}: {body}");

            using var doc = JsonDocument.Parse(body);
            var messageContent = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            if (string.IsNullOrWhiteSpace(messageContent))
                throw new JsonException("OpenRouter response had no message content.");

            var parsed = JsonSerializer.Deserialize<Dictionary<string, string>>(messageContent, JsonOptions)
                ?? throw new JsonException("OpenRouter response content was not a JSON object.");
            return new Dictionary<string, string>(parsed, StringComparer.OrdinalIgnoreCase);
        }

        private static string BuildSystemPrompt(string sourceLang, IReadOnlyList<string> targetLangs, string kind)
        {
            var sourceName = LanguageNames[sourceLang];
            var targetList = string.Join(", ", targetLangs.Select(l => $"\"{l}\" ({LanguageNames[l]})"));

            var styleInstruction = kind == "name"
                ? "The text is a proper noun (a country, city, or place name). Use the name a native speaker of the " +
                  "target language would actually use for it (the conventional exonym), not a literal or transliterated " +
                  "translation - for example the Azerbaijani \"Almaniya\" becomes \"Germany\" in English, not a literal rendering."
                : "The text is a short marketing-style description for a travel app. Translate it naturally and idiomatically, " +
                  "preserving tone and meaning rather than translating word-for-word.";

            return
                $"You are a professional translator. The input text is in {sourceName}. {styleInstruction} " +
                $"Translate it into each of the following languages: {targetList}. " +
                "Respond with ONLY a single JSON object whose keys are exactly the language codes given above and whose " +
                "values are the translated text, with no extra commentary, markdown, or explanation.";
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    }
}
