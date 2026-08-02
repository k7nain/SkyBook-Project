namespace FlyzenApi.Infrastructure.Translation
{
    public class ContentTranslationOptions
    {
        public const string SectionName = "OpenRouter";

        // Empty by default (checked-in appsettings.json) - set via the
        // OPENROUTER_API_KEY env var (mapped to OpenRouter__ApiKey in
        // docker-compose.yml), same pattern as Smtp:Password. Translation
        // simply fails (all langs -> FailedLangs) until this is configured.
        public string ApiKey { get; set; } = string.Empty;

        public string Model { get; set; } = "openai/gpt-4o-mini";

        public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/chat/completions";
    }
}
