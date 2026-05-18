namespace MedicalApp.API.DTOs
{
    public class DoctorUpdateDto
    {
        public string FullName { get; set; } = null!;
        public string Specialization { get; set; } = null!;
        public int ExperienceYears { get; set; }
        public string? Bio { get; set; } // اختياري
        public decimal? SessionPrice { get; set; } // اختياري
    }
}