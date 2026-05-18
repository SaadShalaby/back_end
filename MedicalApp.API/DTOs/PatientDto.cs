namespace MedicalApp.API.DTOs
{
    public class PatientDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public int Age { get; set; }

        public string Gender { get; set; }

        public DateTime? LastSessionDate { get; set; }
    }
}