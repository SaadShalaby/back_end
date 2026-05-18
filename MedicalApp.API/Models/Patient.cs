namespace MedicalApp.API.Models
{
    public class Patient
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;
        public virtual ApplicationUser User { get; set; } = null!;

        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;

        public DateTime RegisteredAt { get; set; } = DateTime.Now;

        public string FullName => User?.FullName ?? "New Patient";
        public string ImageUrl => User?.AvatarUrl ?? "/uploads/profiles/default-patient.png";

        public virtual ICollection<DoctorSession> Sessions { get; set; } = new List<DoctorSession>();
        public virtual ICollection<PatientRecord> Records { get; set; } = new List<PatientRecord>();
    }
}