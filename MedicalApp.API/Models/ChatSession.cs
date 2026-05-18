using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.Models
{
    /// <summary>
    /// Represents a single AI chat session (like a ChatGPT conversation thread).
    /// Each user can have multiple sessions with independent context.
    /// </summary>
    public class ChatSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = "New Chat";

        public bool IsTitleGenerated { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser? User { get; set; }
        public ICollection<BotMessage> Messages { get; set; } = new List<BotMessage>();
    }
}
