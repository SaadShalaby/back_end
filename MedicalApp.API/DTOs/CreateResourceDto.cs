using System.ComponentModel.DataAnnotations;
using MedicalApp.API.Models.Enums;
using Microsoft.AspNetCore.Http;

namespace MedicalApp.API.DTOs
{
    public class CreateResourceDto
    {
        [Required]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        [Required]
        public IFormFile File { get; set; } = default!;

        public int? Duration { get; set; } // seconds (for video)
    }
}
