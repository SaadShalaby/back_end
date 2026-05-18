using System;

namespace MedicalApp.API.DTOs
{
    public class SavedItemResponseDto
    {
        public int SaveId { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public object Data { get; set; } = null!;
    }
}
