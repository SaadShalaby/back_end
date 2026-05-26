using System;

namespace MedicalApp.API.DTOs
{
    public class SessionResponseDto
    {
        public int Id { get; set; }
        public string DoctorId { get; set; } = string.Empty;
        public string DoctorName { get; set; } = string.Empty;
        public string? DoctorAvatar { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? SessionMediaUrl { get; set; }
        public DateTime ScheduledAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string MediaType { get; set; } = string.Empty;
        public int? Duration { get; set; }
    }
}
