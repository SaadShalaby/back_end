using System;
using System.Collections.Generic;

namespace MedicalApp.API.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        // صاحب البوست
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // --- التعديلات الجديدة ---

        public DateTime? UpdatedAt { get; set; }
        public int CommentsCount { get; set; } = 0;
        public int SharesCount { get; set; } = 0;
        public int LikesCount { get; set; } = 0; // إضافة بالمرة عشان الـ Likes

        // العلاقات
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}