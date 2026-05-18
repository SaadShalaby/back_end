namespace MedicalApp.API.DTOs
{
    public class PostResponseDto
    {
        public int Id { get; set; }
        public string Content { get; set; } = default!;
        public string? ImageUrl { get; set; }
        public string UserName { get; set; } = default!;
        public string? UserAvatar { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public bool IsEdited { get; set; }
        public bool IsOwner { get; set; }
        public bool IsSaved { get; set; }
        public bool IsLiked { get; set; }

        public int LikesCount { get; set; }
        public int CommentsCount { get; set; }
    }
}