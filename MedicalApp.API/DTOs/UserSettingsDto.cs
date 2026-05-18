namespace MedicalApp.API.DTOs
{
    public class UserSettingsDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public bool NotificationsEnabled { get; set; }
        public string Language { get; set; } = "en";
    }
}