namespace MedicalApp.API.DTOs
{


    public class ResourceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Type { get; set; } = default!;
        public string? FileUrl { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public int? Duration { get; set; }
        public long FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSaved { get; set; }
    }
}