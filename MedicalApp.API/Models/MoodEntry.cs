namespace MedicalApp.API.Models
{
    public class MoodEntry
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int Value { get; set; } // ?? 1 ?? 5

        public DateTime Date { get; set; } = DateTime.Now;
    }
}