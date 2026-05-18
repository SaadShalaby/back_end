using MedicalApp.API.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace MedicalApp.API.Models
{
    public class DoctorSession
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DoctorId { get; set; } = string.Empty;

        [Required]
        public string PatientId { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        public virtual Patient Patient { get; set; } = null!;

        public string SessionType { get; set; } = "chat";

        public DateTime ScheduledAt { get; set; }

        public decimal Price { get; set; } = 0;

        public string? VideoUrl { get; set; }

        public string? AudioUrl { get; set; }

        public string? PdfUrl { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsStarted { get; set; } = false;

        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected
    }
}