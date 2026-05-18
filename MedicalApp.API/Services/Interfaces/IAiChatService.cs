using MedicalApp.API.DTOs;

namespace MedicalApp.API.Services.Interfaces
{
    public interface IAiChatService
    {
        Task<ChatSessionDto> CreateSessionAsync(string userId, string? title = null);
        Task<PagedResultDto<ChatSessionDto>> GetSessionsAsync(string userId, int page, int pageSize);
        Task<PagedResultDto<BotMessageResponseDto>> GetSessionMessagesAsync(int sessionId, string userId, int page, int pageSize);
        Task<BotMessageResponseDto> SaveMessageAsync(string userId, int sessionId, SendBotMessageDto dto);
        Task<string> GenerateTitleAsync(int sessionId, string userId);
        Task UpdateSessionTitleAsync(int sessionId, string userId, string title);
        Task DeleteSessionAsync(int sessionId, string userId);
        Task<List<BotMessageResponseDto>> GetContextAsync(int sessionId, string userId, int maxMessages = 20);
    }
}
