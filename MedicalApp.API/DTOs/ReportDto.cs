namespace MedicalApp.API.DTOs
{
    public class ReportDto 
    {
        public string PatientId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
    }
}