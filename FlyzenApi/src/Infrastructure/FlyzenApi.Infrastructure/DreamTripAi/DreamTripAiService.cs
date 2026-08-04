using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Repositories;
using FlyzenApi.Infrastructure.Translation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.DreamTripAi
{
    // Same OpenRouter account/config as ChatService/ContentTranslationService (the
    // "OpenRouter" appsettings section) - a third consumer of the same chat-completions
    // endpoint. Reasons over the app's own seeded TripCountry catalog (never lets the
    // model invent a destination the app doesn't actually have content for).
    public class DreamTripAiService : IDreamTripAiService
    {
        private static readonly IReadOnlyDictionary<string, string> LanguageNames = new Dictionary<string, string>
        {
            ["az"] = "Azerbaijani",
            ["en"] = "English",
            ["ru"] = "Russian",
        };

        private readonly HttpClient _httpClient;
        private readonly ContentTranslationOptions _options;
        private readonly ITripCountryRepository _tripCountryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DreamTripAiService> _logger;

        public DreamTripAiService(
            HttpClient httpClient,
            IOptions<ContentTranslationOptions> options,
            ITripCountryRepository tripCountryRepository,
            IUserRepository userRepository,
            ILogger<DreamTripAiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tripCountryRepository = tripCountryRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<DreamTripRecommendResponse> RecommendAsync(Guid userId, DreamTripRecommendRequest request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("Dream Trip AI requested but OpenRouter:ApiKey is not configured.");
                throw new BadRequestException("Xəyal Səyahət Generatoru hazırda konfiqurasiya edilməyib.");
            }

            var countries = (await _tripCountryRepository.GetAllWithCitiesAndPlacesAsync()).ToList();
            if (countries.Count == 0)
            {
                return new DreamTripRecommendResponse
                {
                    Message = "Hazırda kataloqda ölkə yoxdur, tövsiyə vermək mümkün deyil.",
                };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            var language = user is not null && LanguageNames.ContainsKey(user.LanguagePreference)
                ? user.LanguagePreference
                : "az";

            var catalog = BuildCatalog(countries);

            try
            {
                var aiResult = await CallOpenRouterAsync(request, catalog, language, cancellationToken);
                return MatchAgainstCatalog(aiResult, countries, language);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "OpenRouter Dream Trip AI call failed.");
                throw new BadRequestException("Xəyal Səyahət Generatoru hazırda cavab verə bilmir, bir az sonra yenidən cəhd edin.");
            }
        }

        private static string BuildCatalog(IReadOnlyList<TripCountry> countries)
        {
            var lines = countries.Select(country =>
            {
                var categories = country.Cities
                    .SelectMany(city => city.Places)
                    .Select(place => place.Category)
                    .Where(category => !string.IsNullOrWhiteSpace(category))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();

                var categoryText = categories.Count > 0 ? string.Join(", ", categories) : "unknown";
                var cityCount = country.Cities.Count;
                return $"- {country.NameEn ?? country.NameAz} (flag: {country.FlagCode}): {cityCount} cities catalogued; place categories present: {categoryText}";
            });

            return string.Join("\n", lines);
        }

        private async Task<AiRecommendationResult> CallOpenRouterAsync(
            DreamTripRecommendRequest request, string catalog, string language, CancellationToken cancellationToken)
        {
            var languageName = LanguageNames[language];
            var interests = string.Join(", ", request.Interests);

            var systemPrompt =
                "You are a travel-recommendation engine for the SkyBook flight booking app. You must recommend " +
                "ONLY countries that appear in the catalog given below - never suggest a country that is not listed, " +
                "even if it would otherwise fit well. Pick between 1 and 3 countries that best fit the user's budget, " +
                "travel month/season, and interests, using the listed place categories as a signal for what each " +
                "country actually offers. If nothing in the catalog is a good fit, it's fine to return fewer countries " +
                "(even zero) and explain why in \"message\", rather than forcing a bad match. " +
                $"Respond in {languageName}. Respond with ONLY a single JSON object of the shape " +
                "{\"recommendations\":[{\"countryName\":\"<exact name from the catalog>\",\"reason\":\"<1-2 sentence explanation>\"}],\"message\":\"<optional note, or null>\"}, " +
                "with no extra commentary or markdown.\n\nCatalog:\n" + catalog;

            var userPrompt =
                $"Budget: {request.BudgetLevel}\nTravel month/season: {request.TravelMonth}\nInterests: {interests}";

            var requestBody = new
            {
                model = _options.Model,
                messages = new object[]
                {
                    new { role = "system", content = systemPrompt },
                    new { role = "user", content = userPrompt },
                },
                response_format = new { type = "json_object" },
            };

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
            {
                Content = JsonContent.Create(requestBody),
            };
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(25));

            using var response = await _httpClient.SendAsync(httpRequest, cts.Token);
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

            return JsonSerializer.Deserialize<AiRecommendationResult>(messageContent, JsonOptions)
                ?? throw new JsonException("OpenRouter response content was not a JSON object.");
        }

        private static DreamTripRecommendResponse MatchAgainstCatalog(AiRecommendationResult aiResult, IReadOnlyList<TripCountry> countries, string language)
        {
            var recommendations = new List<DreamTripRecommendationDto>();
            var seen = new HashSet<Guid>();

            foreach (var item in aiResult.Recommendations ?? new())
            {
                if (string.IsNullOrWhiteSpace(item.CountryName))
                    continue;

                var match = countries.FirstOrDefault(c =>
                    string.Equals(c.NameEn, item.CountryName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.NameAz, item.CountryName, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.NameRu, item.CountryName, StringComparison.OrdinalIgnoreCase));

                // Defensive: never surface a country the AI hallucinated outside the catalog.
                if (match is null || !seen.Add(match.Id))
                    continue;

                recommendations.Add(new DreamTripRecommendationDto
                {
                    Country = match.ToDto(),
                    Reason = item.Reason?.Trim() ?? string.Empty,
                });

                if (recommendations.Count >= 3)
                    break;
            }

            var message = aiResult.Message;
            if (recommendations.Count == 0 && string.IsNullOrWhiteSpace(message))
            {
                message = language switch
                {
                    "en" => "We couldn't find a strong match for your answers - try broadening your budget or interests.",
                    "ru" => "Мы не смогли найти подходящий вариант - попробуйте расширить бюджет или интересы.",
                    _ => "Cavablarınıza uyğun güclü bir seçim tapa bilmədik - büdcəni və ya marağı genişləndirməyi sınayın.",
                };
            }

            return new DreamTripRecommendResponse { Recommendations = recommendations, Message = message };
        }

        private class AiRecommendationResult
        {
            public List<AiRecommendationItem>? Recommendations { get; set; }
            public string? Message { get; set; }
        }

        private class AiRecommendationItem
        {
            [JsonPropertyName("countryName")]
            public string? CountryName { get; set; }
            public string? Reason { get; set; }
        }

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    }
}
