namespace MedicalApp.API.DTOs
{
    public class CreateReportDto
    {
        public string PatientId { get; set; } = null!;
        public string Type { get; set; } = null!; // مثلاً: أشعة، تحاليل، تقرير جلسة
        public string FileUrl { get; set; } = null!;
    }
}
