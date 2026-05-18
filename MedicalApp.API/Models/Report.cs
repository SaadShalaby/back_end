using System;

namespace MedicalApp.API.Models
{
    public class Report
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = null!;
        public string PatientId { get; set; } = null!;
        public string Type { get; set; } = null!;
        public string FileUrl { get; set; } = null!;
        public DateTime ReportDate { get; set; } = DateTime.Now;
    }
}