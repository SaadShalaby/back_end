using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace MedicalApp.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public bool NotificationsEnabled { get; set; } = true;

        public string Language { get; set; } = "en";

        public int SessionsCompleted { get; set; } = 0;
        public int ExercisesCompleted { get; set; } = 0;
        public int ActiveDays { get; set; } = 0;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}