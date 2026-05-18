namespace MedicalApp.API.Models
{
    public class Doctor
    {
        public int Id { get; set; }

        // ربط الدكتور بحساب المستخدم الأساسي (عشان الـ Auth والـ Login)
        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!; // Navigation Property

        public string Name { get; set; } = null!;
        public string NationalNumber { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        public double Rating { get; set; }
        public string Bio { get; set; } = null!;    // وصف مختصر للدكتور

        // إضافة: سعر الكشف أو الجلسة (لأننا ضفنا Price في المايجريشن اللي فات)
        public decimal SessionPrice { get; set; }

        // ملحوظة: الصورة يفضل نسحبها من ApplicationUser.ProfileImage 
        // عشان لو غير صورته في البروفايل تتغير في كل مكان في الأبلكيشن
        public string? ImageUrl { get; set; }
    }
}