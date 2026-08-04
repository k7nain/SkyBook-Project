using System.ComponentModel.DataAnnotations;

namespace FlyzenApi.Application.DTOs
{
    public class ChatMessageDto
    {
        public string Role { get; set; } = string.Empty; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public class ChatRequest
    {
        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        // Prior turns in this conversation, oldest first. Trimmed server-side
        // (see ChatService.MaxHistoryMessages) so a long-running chat can't
        // make the OpenRouter request grow unbounded.
        public List<ChatMessageDto>? History { get; set; }
    }

    public class ChatResponse
    {
        public string Reply { get; set; } = string.Empty;
    }
}
