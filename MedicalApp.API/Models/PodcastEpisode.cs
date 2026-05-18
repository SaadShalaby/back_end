namespace MedicalApp.API.Models
{
    public class PodcastEpisode
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public string AudioUrl { get; set; }   // Streaming URL
        public string CoverImageUrl { get; set; }

        public int DurationInSeconds { get; set; }

        public DateTime PublishDate { get; set; } = DateTime.Now;

        public bool IsPublished { get; set; } = true;
    }
}
