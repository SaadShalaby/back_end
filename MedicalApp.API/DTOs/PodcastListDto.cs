namespace MedicalApp.API.DTOs
{
    public class PodcastListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string AudioUrl { get; set; }
        public string CoverImageUrl { get; set; }
        public int DurationInSeconds { get; set; }
        public DateTime PublishDate { get; set; }
    }
}
