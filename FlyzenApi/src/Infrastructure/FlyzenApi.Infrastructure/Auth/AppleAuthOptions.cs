namespace FlyzenApi.Infrastructure.Auth
{
    public class AppleAuthOptions
    {
        public const string SectionName = "Auth:Apple";

        public string[] ClientIds { get; set; } = Array.Empty<string>();
    }
}
