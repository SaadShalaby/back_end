namespace MedicalApp.API.Models
{
    public class PodcastEpisode
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string FileUrl { get; set; } = default!;
        public string? FileName { get; set; }
        public string? MimeType { get; set; }
        public long FileSize { get; set; }

        public string? CoverImageUrl { get; set; }

        public int? Duration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}
