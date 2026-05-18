namespace MedicalApp.API.Models
{
    public class SupportTicket
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string Subject { get; set; }
        public string Message { get; set; }

        public string Status { get; set; } = "Open"; // Open, Closed

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
