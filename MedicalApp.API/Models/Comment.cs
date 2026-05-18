using System;
using System.Text.Json.Serialization;

namespace MedicalApp.API.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; } = string.Empty;

        // صاحب التعليق
        public string UserId { get; set; } = string.Empty;

        [JsonIgnore] // بتخلي الـ API يتجاهل طلبها من بتاع الموبايل وهو بيبعت الكومنت
        public virtual ApplicationUser? User { get; set; } // ضفنا ? عشان متبقاش إجبارية

        // التعليق تابع لأي بوست؟
        public int PostId { get; set; }

        [JsonIgnore] // نفس الكلام هنا
        public virtual Post? Post { get; set; } // ضفنا ? عشان متبقاش إجبارية

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }
    }
}
