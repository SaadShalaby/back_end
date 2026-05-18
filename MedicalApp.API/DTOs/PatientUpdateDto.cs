namespace MedicalApp.API.DTOs
{
    public class PatientUpdateDto
    {
        public string FullName { get; set; } = null!;
        public int Age { get; set; }
        public string Gender { get; set; } = null!;
    }
}
