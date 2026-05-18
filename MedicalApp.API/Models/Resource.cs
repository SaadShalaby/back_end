using System;
using System.ComponentModel.DataAnnotations;
using MedicalApp.API.Models.Enums;

namespace MedicalApp.API.Models
{
    public class Resource
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = default!;

        public string? Description { get; set; }

        public string? CoverImageUrl { get; set; }

        [Required]
        public ResourceType Type { get; set; }

        public string? Url { get; set; }

        public int? Duration { get; set; } // seconds (for video)

        public double? FileSize { get; set; } // MB (for pdf)

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}