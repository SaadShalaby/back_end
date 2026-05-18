namespace MedicalApp.API.DTOs
{


    public class ResourceDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string? Description { get; set; }
        public string? CoverImageUrl { get; set; }
        public string Type { get; set; } = default!;
        public string? Url { get; set; }
        public int? Duration { get; set; }
        public double? FileSize { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsSaved { get; set; }
    }
}