namespace MedicalApp.API.DTOs
{
    public class DoctorDto
    {
        public int Id { get; set; }
        public string UserId { get; set; } // تم إضافته عشان الموبايل يستخدمه في الشات

        // الاسم الكامل للدكتور
        public string Name { get; set; } = string.Empty;

        // التخصص (مثلاً: أخصائي نفسي)
        public string Specialization { get; set; } = string.Empty;

        // سنين الخبرة
        public int ExperienceYears { get; set; }

        // التقييم من 5
        public double Rating { get; set; }

        // رابط الصورة الشخصية
        public string ImageUrl { get; set; } = string.Empty;

        // إضافة: نبذة عن الدكتور (Bio) مهمة جداً لشاشة البروفايل
        public string Bio { get; set; } = string.Empty;

        // إضافة: سعر الجلسة (عشان المريض يعرف التكلفة)
        public decimal SessionPrice { get; set; }

        // إضافة: عدد المراجعات/التقييمات (اختياري)
        public int ReviewsCount { get; set; }
    }
}