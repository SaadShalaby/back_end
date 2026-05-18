using MedicalApp.API.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.Models
{
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } = string.Empty;

        [Required]
        public string ReceiverId { get; set; } = string.Empty;

        [Required]
        public string ConversationId { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string? Content { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Text;

        /// <summary>URL of the attached file (voice, image, file)</summary>
        public string? AttachmentUrl { get; set; }

        /// <summary>Original file name for downloads</summary>
        public string? AttachmentName { get; set; }

        public bool IsPinned { get; set; } = false;

        public bool IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.Now;
        // Note: No navigation properties to avoid FK conflicts with existing data
    }
}
