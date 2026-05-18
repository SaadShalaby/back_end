namespace MedicalApp.API.DTOs
{
    public class ConversationDto
    {
        public string ConversationId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? UserImage { get; set; }
        public string? LastMessage { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
        public bool IsFavorite { get; set; }
    }
}