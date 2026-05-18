using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.Models
{
    /// <summary>Stores which conversations a user has marked as favorite</summary>
    public class FavoriteChat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ConversationId { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public ApplicationUser? User { get; set; }
    }
}
