using Microsoft.AspNetCore.Http; 

namespace MedicalApp.API.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }

        public IFormFile? ProfileImage { get; set; }
    }
}