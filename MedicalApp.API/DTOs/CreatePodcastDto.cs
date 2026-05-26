using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace MedicalApp.API.DTOs
{
    public class CreatePodcastDto
    {
        [Required]
        public string Title { get; set; } = default!;

        [Required]
        public string Description { get; set; } = default!;

        [Required]
        public IFormFile File { get; set; } = default!;

        public string? CoverImageUrl { get; set; }

        public int? Duration { get; set; }
    }
}
