using MedicalApp.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.DTOs
{
    public class SendBotMessageDto
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Sender { get; set; } = string.Empty; // "Patient" or "Depo"

        public MessageType MessageType { get; set; } = MessageType.Text;

        public string? Emotion { get; set; }

        public IFormFile? Attachment { get; set; }
    }

    public class BotMessageResponseDto
    {
        public int Id { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public MessageType MessageType { get; set; }
        public string? Emotion { get; set; }
        public string? AttachmentUrl { get; set; }
        public DateTime SentAt { get; set; }
    }

    public class ChatSessionDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public bool IsTitleGenerated { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? LastMessage { get; set; }
        public int MessageCount { get; set; }
    }

    public class UpdateSessionTitleDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;
    }

    public class CreateChatSessionDto
    {
        [MaxLength(200)]
        public string? Title { get; set; }
    }
}
