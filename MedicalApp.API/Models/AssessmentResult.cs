namespace MedicalApp.API.Models
{
    public class AssessmentResult
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string AssessmentName { get; set; }

        public int Percentage { get; set; }

        public string SymptomLevel { get; set; }

        public string AnswersJson { get; set; } // ????? ????????

        public string Recommendation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}