using System;

namespace MedicalApp.API.Models
{
    public class Like
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public int PostId { get; set; }
        public virtual Post Post { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}