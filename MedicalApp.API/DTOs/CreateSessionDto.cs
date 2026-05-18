using Microsoft.AspNetCore.Http;

namespace MedicalApp.API.DTOs
{
    public class CreateSessionDto
    {

        // بنحتاج الـ PatientId عشان نعرف الجلسة دي بتاعة مين بالظبط

        public string SessionType { get; set; } = "chat"; // chat / video / audio / pdf

        public DateTime ScheduledAt { get; set; }


        public decimal? Price { get; set; } // سعر الجلسة (اللي في الصورة)

        public string? VideoUrl { get; set; } // رابط فيديو الـ Resources

        public string? AudioUrl { get; set; } // رابط ملف الـ Podcast

        public string? PdfUrl { get; set; } // رابط الكتاب الـ PDF

        public string? ImageUrl { get; set; } // رابط صورة الـ Chat

        public IFormFile? MediaFile { get; set; } // الملف المرفوع (فيديو، صوت، pdf، صورة)
    }
}