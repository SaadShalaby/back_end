using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalApp.API.Models
{
    public class SavedItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty; // e.g., "post", "resource"

        [Required]
        public int ItemId { get; set; }

        public DateTime SavedAt { get; set; } = DateTime.Now;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;
    }
}
