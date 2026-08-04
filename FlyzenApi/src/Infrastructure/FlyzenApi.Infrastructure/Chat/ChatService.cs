using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FlyzenApi.Application.DTOs;
using FlyzenApi.Application.Exceptions;
using FlyzenApi.Application.Interfaces.Services;
using FlyzenApi.Infrastructure.Translation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlyzenApi.Infrastructure.Chat
{
    // Same OpenRouter account/config as ContentTranslationService (the
    // "OpenRouter" appsettings section) - this is a second consumer of the
    // same chat-completions endpoint, not a separate integration.
    public class ChatService : IChatService
    {
        private const string SystemPrompt =
            "You are the SkyBook Assistant, a friendly and knowledgeable travel assistant for the SkyBook flight " +
            "booking platform. Help users with questions about flights, baggage allowances, check-in, booking, " +
            "cancellations, refunds, seat selection, meal options, and general travel advice. Keep answers concise " +
            "and conversational (2-4 sentences unless the question genuinely needs more detail). If asked about " +
            "something unrelated to travel or SkyBook, politely say that's outside what you can help with and steer " +
            "back to travel topics. Always reply in the same language the user's latest message is written in.";

        // Caps how much prior conversation gets resent on every turn - keeps
        // the request (and cost) from growing unbounded in a long chat.
        private const int MaxHistoryMessages = 10;

        private readonly HttpClient _httpClient;
        private readonly ContentTranslationOptions _options;
        private readonly ILogger<ChatService> _logger;

        public ChatService(HttpClient httpClient, IOptions<ContentTranslationOptions> options, ILogger<ChatService> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GetReplyAsync(string message, IReadOnlyList<ChatMessageDto>? history, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                _logger.LogWarning("Chat requested but OpenRouter:ApiKey is not configured.");
                throw new BadRequestException("Kömək Mərkəzi hazırda konfiqurasiya edilməyib.");
            }

            var messages = new List<object> { new { role = "system", content = SystemPrompt } };
            if (history is { Count: > 0 })
            {
                foreach (var turn in history.TakeLast(MaxHistoryMessages))
                {
                    var role = turn.Role == "assistant" ? "assistant" : "user";
                    messages.Add(new { role, content = turn.Content });
                }
            }
            messages.Add(new { role = "user", content = message });

            var requestBody = new
            {
                model = _options.Model,
                messages,
                max_tokens = 400,
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
            {
                Content = JsonContent.Create(requestBody),
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(25));

            try
            {
                using var response = await _httpClient.SendAsync(request, cts.Token);
                var body = await response.Content.ReadAsStringAsync(cts.Token);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"OpenRouter returned {(int)response.StatusCode}: {body}");

                using var doc = JsonDocument.Parse(body);
                var reply = doc.RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(reply))
                    throw new JsonException("OpenRouter response had no message content.");

                return reply.Trim();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
            {
                _logger.LogWarning(ex, "OpenRouter chat call failed.");
                throw new BadRequestException("Kömək Mərkəzi hazırda cavab verə bilmir, bir az sonra yenidən cəhd edin.");
            }
        }
    }
}
