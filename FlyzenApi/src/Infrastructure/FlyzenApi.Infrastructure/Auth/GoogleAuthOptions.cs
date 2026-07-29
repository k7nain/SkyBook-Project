namespace FlyzenApi.Infrastructure.Auth
{
    public class GoogleAuthOptions
    {
        public const string SectionName = "Auth:Google";

        public string[] ClientIds { get; set; } = Array.Empty<string>();
    }
}
