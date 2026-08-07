using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Application.Mapping;
using FlyzenApi.Domain.Entities;
using FlyzenApi.Domain.Enums;
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

        // Recommendations regenerate at most this often per user, unless their
        // SignatureHash changes sooner (see IsCacheFresh) - the cost/performance
        // guard against calling OpenRouter on every single Home page load.
        private static readonly TimeSpan ForYouCacheDuration = TimeSpan.FromHours(24);

        // Below this many distinct signal countries, there isn't enough real
        // behavior to personalize against - BuildPopularFallbackAsync runs instead
        // of spending an OpenRouter call on what would essentially be a guess.
        private const int MinSignalCountriesForAi = 1;

        private readonly HttpClient _httpClient;
        private readonly ContentTranslationOptions _options;
        private readonly ITripCountryRepository _tripCountryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ISearchLogRepository _searchLogRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITravelJournalRepository _travelJournalRepository;
        private readonly IUserRecommendationCacheRepository _cacheRepository;
        private readonly ILogger<DreamTripAiService> _logger;

        public DreamTripAiService(
            HttpClient httpClient,
            IOptions<ContentTranslationOptions> options,
            ITripCountryRepository tripCountryRepository,
            IUserRepository userRepository,
            ISearchLogRepository searchLogRepository,
            IBookingRepository bookingRepository,
            ITravelJournalRepository travelJournalRepository,
            IUserRecommendationCacheRepository cacheRepository,
            ILogger<DreamTripAiService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _tripCountryRepository = tripCountryRepository;
            _userRepository = userRepository;
            _searchLogRepository = searchLogRepository;
            _bookingRepository = bookingRepository;
            _travelJournalRepository = travelJournalRepository;
            _cacheRepository = cacheRepository;
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

        public async Task<DreamTripRecommendResponse> RecommendForYouAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var signals = await GatherBehaviorSignalsAsync(userId);
            var signature = BuildSignature(signals);

            var cached = await _cacheRepository.GetByUserIdAsync(userId);
            if (cached is not null && cached.SignatureHash == signature && DateTime.UtcNow - cached.GeneratedAt < ForYouCacheDuration)
            {
                var cachedResponse = JsonSerializer.Deserialize<DreamTripRecommendResponse>(cached.ResponseJson, JsonOptions);
                if (cachedResponse is not null)
                    return cachedResponse;
            }

            var countries = (await _tripCountryRepository.GetAllWithCitiesAndPlacesAsync()).ToList();
            var user = await _userRepository.GetByIdAsync(userId);
            var language = user is not null && LanguageNames.ContainsKey(user.LanguagePreference)
                ? user.LanguagePreference
                : "az";

            if (countries.Count == 0)
                return new DreamTripRecommendResponse { Message = "Hazırda kataloqda ölkə yoxdur, tövsiyə vermək mümkün deyil.", IsPersonalized = false };

            DreamTripRecommendResponse response;

            // New user (or one with too little tracked activity yet) - a passive
            // feature like this shouldn't spend an OpenRouter call producing what
            // would amount to a guess; generally popular destinations are a more
            // honest default until there's real behavior to personalize against.
            if (signals.CountryNames.Count < MinSignalCountriesForAi || string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                response = await BuildPopularFallbackAsync(countries, language);
            }
            else
            {
                try
                {
                    var catalog = BuildCatalog(countries);
                    var aiResult = await CallOpenRouterForYouAsync(signals, catalog, language, cancellationToken);
                    response = MatchAgainstCatalog(aiResult, countries, language, maxResults: 5);
                    if (response.Recommendations.Count == 0)
                        response = await BuildPopularFallbackAsync(countries, language);
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    _logger.LogWarning(ex, "OpenRouter For You AI call failed; falling back to popular destinations.");
                    response = await BuildPopularFallbackAsync(countries, language);
                }
            }

            await _cacheRepository.UpsertAsync(new UserRecommendationCache
            {
                UserId = userId,
                ResponseJson = JsonSerializer.Serialize(response, JsonOptions),
                SignatureHash = signature,
                GeneratedAt = DateTime.UtcNow,
            });

            return response;
        }

        private record BehaviorSignals(List<string> CountryNames, List<string> SearchedCountries, List<string> BookedCountries, List<string> JournaledCountries);

        private async Task<BehaviorSignals> GatherBehaviorSignalsAsync(Guid userId)
        {
            var searchLogs = await _searchLogRepository.GetRecentByUserIdAsync(userId, days: 90);
            var bookings = await _bookingRepository.GetByUserIdAsync(userId);
            var journals = await _travelJournalRepository.GetByUserIdAsync(userId);

            var searchedCountries = searchLogs.Select(l => l.ToCity.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var bookedCountries = bookings.Where(b => b.Status != BookingStatus.Cancelled).Select(b => b.Flight.ArrivalCity.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var journaledCountries = journals.Select(j => j.DestinationCity.Country).Where(c => !string.IsNullOrWhiteSpace(c)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var allCountries = searchedCountries.Concat(bookedCountries).Concat(journaledCountries)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return new BehaviorSignals(allCountries, searchedCountries, bookedCountries, journaledCountries);
        }

        // Human-readable and stable across runs (sorted) - doubles as the cache
        // invalidation key (UserRecommendationCache.SignatureHash) and as a plain
        // string is easy enough to debug without needing an actual hash function.
        private static string BuildSignature(BehaviorSignals signals) =>
            string.Join("|", signals.CountryNames);

        private async Task<AiRecommendationResult> CallOpenRouterForYouAsync(BehaviorSignals signals, string catalog, string language, CancellationToken cancellationToken)
        {
            var languageName = LanguageNames[language];

            var systemPrompt =
                "You are a personalized-recommendation engine for the SkyBook flight booking app, the passive " +
                "counterpart to its quiz-based Dream Trip generator - this one is driven by the user's own past " +
                "behavior instead of quiz answers. You must recommend ONLY countries that appear in the catalog " +
                "given below - never suggest a country that is not listed. Given a summary of the user's actual " +
                "search/booking/travel-journal history, pick between 3 and 5 countries that are similar to, or a " +
                "natural next step from, what they've already shown interest in (e.g. a similar coastline, culture, " +
                "or activity type) - do not just repeat a country they've already been to unless there's a genuinely " +
                "compelling reason. Each reason MUST explicitly reference the specific behavior that motivated it " +
                $"(e.g. \"Since you searched for Turkey...\"). Respond in {languageName}. Respond with ONLY a single " +
                "JSON object of the shape {\"recommendations\":[{\"countryName\":\"<exact name from the catalog>\"," +
                "\"reason\":\"<1-2 sentence explanation tied to their behavior>\"}],\"message\":\"<optional note, or null>\"}, " +
                "with no extra commentary or markdown.\n\nCatalog:\n" + catalog;

            var userPromptParts = new List<string>();
            if (signals.SearchedCountries.Count > 0)
                userPromptParts.Add($"Has searched flights to: {string.Join(", ", signals.SearchedCountries)}");
            if (signals.BookedCountries.Count > 0)
                userPromptParts.Add($"Has booked/completed trips to: {string.Join(", ", signals.BookedCountries)}");
            if (signals.JournaledCountries.Count > 0)
                userPromptParts.Add($"Has written travel journals about: {string.Join(", ", signals.JournaledCountries)}");
            var userPrompt = string.Join("\n", userPromptParts);

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

        // "Generally popular" = the destination countries with the most
        // non-cancelled bookings app-wide (see IBookingRepository.
        // GetPopularDestinationCountriesAsync); if the app has no bookings yet
        // either, falls back further to just the first few catalog countries so
        // the section is never simply empty for a brand new deployment.
        private async Task<DreamTripRecommendResponse> BuildPopularFallbackAsync(IReadOnlyList<TripCountry> countries, string language)
        {
            var reasonText = language switch
            {
                "en" => "One of our travelers' most popular destinations right now.",
                "ru" => "Одно из самых популярных направлений среди наших путешественников.",
                _ => "Hazırda səyahətçilərimiz arasında ən populyar istiqamətlərdən biri.",
            };

            var popularCountryNames = await _bookingRepository.GetPopularDestinationCountriesAsync(5);
            var recommendations = new List<DreamTripRecommendationDto>();
            var seen = new HashSet<Guid>();

            foreach (var name in popularCountryNames)
            {
                var match = countries.FirstOrDefault(c =>
                    string.Equals(c.NameEn, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.NameAz, name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.NameRu, name, StringComparison.OrdinalIgnoreCase));

                if (match is null || !seen.Add(match.Id))
                    continue;

                recommendations.Add(new DreamTripRecommendationDto { Country = match.ToDto(), Reason = reasonText });
                if (recommendations.Count >= 5)
                    break;
            }

            // Nothing booked anywhere yet (brand new deployment) - still show
            // something rather than an empty section.
            if (recommendations.Count == 0)
            {
                foreach (var country in countries.Take(3))
                    recommendations.Add(new DreamTripRecommendationDto { Country = country.ToDto(), Reason = reasonText });
            }

            return new DreamTripRecommendResponse { Recommendations = recommendations, IsPersonalized = false };
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

        private static DreamTripRecommendResponse MatchAgainstCatalog(AiRecommendationResult aiResult, IReadOnlyList<TripCountry> countries, string language, int maxResults = 3)
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

                if (recommendations.Count >= maxResults)
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
