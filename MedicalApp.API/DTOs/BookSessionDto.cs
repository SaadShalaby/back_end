namespace MedicalApp.API.DTOs
{
    public class BookSessionDto
    {
        public int DoctorId { get; set; }
        public DateTime SessionDate { get; set; }
        public string SessionType { get; set; } // فيديو أو شات
    }
}