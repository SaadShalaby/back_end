using MedicalApp.API.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApp.API.Models
{
    public class BotMessage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PatientId { get; set; } = string.Empty;

        /// <summary>Links this message to a specific ChatSession</summary>
        public int SessionId { get; set; }

        [Required]
        [MaxLength(50)]
        public string Sender { get; set; } = string.Empty; // "Patient" or "Depo"

        [Required]
        public string Message { get; set; } = string.Empty;

        /// <summary>Stores the facial expression / emotion of the bot</summary>
        [MaxLength(50)]
        public string? Emotion { get; set; }

        public MessageType MessageType { get; set; } = MessageType.Text;

        /// <summary>URL for voice/image attachments in bot chat</summary>
        public string? AttachmentUrl { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;

        // Navigation
        [ForeignKey("PatientId")]
        public ApplicationUser? Patient { get; set; }

        [ForeignKey("SessionId")]
        public ChatSession? Session { get; set; }
    }
}
