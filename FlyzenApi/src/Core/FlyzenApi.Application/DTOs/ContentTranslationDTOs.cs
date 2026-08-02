using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class ContentTranslateRequest
    {
        [Required, MaxLength(2000)]
        public string Text { get; set; } = string.Empty;

        // "az" | "en" | "ru"
        [Required, MaxLength(2)]
        public string SourceLang { get; set; } = string.Empty;

        [Required, MinLength(1)]
        public List<string> TargetLangs { get; set; } = new();

        // "name" (proper noun - use the conventional target-language exonym,
        // not a literal translation) or "description" (natural prose).
        [Required]
        public string Kind { get; set; } = string.Empty;
    }

    public class ContentTranslateResponse
    {
        // lang code -> translated text, present only for languages that succeeded.
        public Dictionary<string, string> Translations { get; set; } = new();

        // Langs the caller asked for but didn't get back - the admin fills these
        // in manually instead of the request failing outright.
        public List<string> FailedLangs { get; set; } = new();
    }
}
