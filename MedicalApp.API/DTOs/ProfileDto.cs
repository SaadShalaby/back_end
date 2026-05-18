namespace MedicalApp.API.DTOs
{
    public class ProfileDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }

        public int SessionsCompleted { get; set; }
        public int ExercisesCompleted { get; set; }
        public int ActiveDays { get; set; }
    }
}