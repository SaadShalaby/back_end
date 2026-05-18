namespace MedicalApp.API.Models
{
    public class Faq
    {
        public int Id { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
