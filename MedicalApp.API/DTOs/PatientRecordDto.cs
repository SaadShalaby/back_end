namespace MedicalApp.API.DTOs
{
    public class RecordDto
    {
        public string PatientId { get; set; } = string.Empty; // ✅ تم الإرجاع لـ string ليتوافق مع الموديل الجديد
        public string Diagnosis { get; set; } = null!;
        public string Notes { get; set; } = null!;
        public string TreatmentPlan { get; set; } = null!;
    }
}