using System;

namespace MedicalApp.API.Models
{
    public class PatientRecord
    {
        public int Id { get; set; }
        public string PatientId { get; set; } = string.Empty; // ✅ تم الإرجاع لـ string ليتوافق مع UserId في Patient
        public virtual Patient Patient { get; set; } = null!;
        public string DoctorId { get; set; } = string.Empty;
        public string Diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? TreatmentPlan { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}