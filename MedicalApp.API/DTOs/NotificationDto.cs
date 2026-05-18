namespace MedicalApp.API.DTOs
{
    public class NotificationDto
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public string Type { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}