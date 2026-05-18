using MedicalApp.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.DTOs
{
    public class MessageDto
    {
        public int Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string ReceiverId { get; set; } = string.Empty;
        public string? Message { get; set; }
        public MessageType MessageType { get; set; }
        public string? AttachmentUrl { get; set; }
        public string? AttachmentName { get; set; }
        public bool IsPinned { get; set; }
        public bool IsRead { get; set; }
        public DateTime SentAt { get; set; }
    }

    /// <summary>Used for sending TEXT messages — JSON body</summary>
    public class SendMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        public string? Message { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Text;
    }

    /// <summary>Used for sending VOICE/IMAGE/FILE — multipart/form-data</summary>
    public class SendFileMessageDto
    {
        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Image;

        [Required]
        public IFormFile Attachment { get; set; } = null!;
    }
}