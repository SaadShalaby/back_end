using MedicalApp.API.DTOs;

namespace MedicalApp.API.Services.Interfaces
{
    public interface IMessageService
    {
        Task<PagedResultDto<ConversationDto>> GetConversationsAsync(string userId, int page, int pageSize);
        Task<PagedResultDto<MessageDto>> GetMessagesAsync(string conversationId, string userId, int page, int pageSize);
        Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto);
        Task<MessageDto> SendFileMessageAsync(string senderId, SendFileMessageDto dto);
        Task MarkAsReadAsync(string conversationId, string userId);
        Task<int> GetUnreadCountAsync(string userId);
        Task<MessageDto> PinMessageAsync(int messageId, string userId);
        Task<MessageDto> UnpinMessageAsync(int messageId, string userId);
        Task<List<MessageDto>> GetPinnedMessagesAsync(string conversationId, string userId);
        Task<bool> ToggleFavoriteAsync(string conversationId, string userId);
        Task<List<ConversationDto>> GetFavoritesAsync(string userId);
    }
}
