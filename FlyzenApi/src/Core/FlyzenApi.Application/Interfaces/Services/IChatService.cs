using FlyzenApi.Application.DTOs;

namespace FlyzenApi.Application.Interfaces.Services
{
    public interface IChatService
    {
        Task<string> GetReplyAsync(string message, IReadOnlyList<ChatMessageDto>? history, CancellationToken cancellationToken = default);
    }
}
