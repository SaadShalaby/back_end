//using MedicalApp.API.Data;
//using MedicalApp.API.DTOs;
//using MedicalApp.API.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace MedicalApp.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    [Authorize]
//    public class PostsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;

//        public PostsController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create([FromForm] string content, IFormFile? image)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            string? imagePath = null;

//            if (image != null)
//            {
//                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
//                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
//                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
//                var filePath = Path.Combine(uploadsFolder, fileName);

//                using (var stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await image.CopyToAsync(stream);
//                }
//                imagePath = "/images/" + fileName;
//            }

//            var post = new Post
//            {
//                Content = content,
//                ImageUrl = imagePath,
//                UserId = userId!,
//                CreatedAt = DateTime.Now
//            };

//            _context.Posts.Add(post);
//            await _context.SaveChangesAsync();

//            return Ok(post);
//        }

//        [HttpGet]
//        public async Task<IActionResult> GetAll()
//        {
//            var posts = await _context.Posts
//                .Include(p => p.User)
//                .OrderByDescending(p => p.CreatedAt)
//                .Select(p => new
//                {
//                    p.Id,
//                    p.Content,
//                    p.ImageUrl,
//                    UserName = p.User.FullName,
//                    UserAvatar = p.User.AvatarUrl ?? "/images/default-user.png",
//                    p.CreatedAt,
//                    LikesCount = _context.Likes.Count(l => l.PostId == p.Id),
//                    CommentsCount = _context.Comments.Count(c => c.PostId == p.Id)
//                })
//                .ToListAsync();

//            return Ok(posts);
//        }

//        // ?? ??????? ???: ?????? string ???? ??????? ??? ??????? ???? ???? ??? Validation
//        [HttpPut("{id}")]
//        public async Task<IActionResult> Edit(int id, [FromBody] string newContent)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            var post = await _context.Posts.FindAsync(id);

//            if (post == null) return NotFound();
//            if (post.UserId != userId) return Forbid();

//            post.Content = newContent;
//            post.UpdatedAt = DateTime.Now; // ????? ??? ???????

//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Post updated", content = post.Content });
//        }

//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            var post = await _context.Posts.FindAsync(id);

//            if (post == null) return NotFound();
//            if (post.UserId != userId) return Forbid();

//            _context.Posts.Remove(post);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Deleted Successfully" });
//        }

//        // ==========================
//        // ?? ???? ????????? (Comments)
//        // ==========================

//        [HttpGet("{postId}/comments")]
//        public async Task<IActionResult> GetComments(int postId)
//        {
//            var comments = await _context.Comments
//                .Where(c => c.PostId == postId)
//                .Include(c => c.User)
//                .OrderBy(c => c.CreatedAt)
//                .Select(c => new
//                {
//                    c.Id,
//                    c.Content,
//                    UserName = c.User != null ? c.User.FullName : "Unknown User",
//                    UserAvatar = (c.User != null && c.User.AvatarUrl != null) ? c.User.AvatarUrl : "/images/default-user.png",
//                    c.CreatedAt,
//                    c.UserId
//                })
//                .ToListAsync();

//            return Ok(comments);
//        }

//        [HttpPost("{postId}/comments")]
//        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentDto dto)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//            // ???? ?? ?????? ????? ??? ????? ???????
//            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
//            if (!postExists) return NotFound("Post not found");

//            var comment = new Comment
//            {
//                PostId = postId,
//                UserId = userId!,
//                Content = dto.Text,
//                CreatedAt = DateTime.Now
//            };

//            _context.Comments.Add(comment);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Comment added!" });
//        }

//        [HttpDelete("comments/{commentId}")]
//        public async Task<IActionResult> DeleteComment(int commentId)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            var comment = await _context.Comments.FindAsync(commentId);

//            if (comment == null) return NotFound();
//            if (comment.UserId != userId) return Forbid();

//            _context.Comments.Remove(comment);
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Comment deleted" });
//        }
//    }
//}
using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using MedicalApp.API.Hubs;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PostsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public PostsController(AppDbContext context, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub> notificationHub)
        {
            _context = context;
            _userManager = userManager;
            _notificationHub = notificationHub;
        }

        // ==========================
        // ?? Egypt Time Helper
        // ==========================
        private DateTime GetEgyptTime()
        {
            var egyptTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.Now,
                egyptTimeZone
            );
        }

        // ==========================
        // ?? Create Post
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] string content, IFormFile? image)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            string? imagePath = null;

            if (image != null)
            {
                var uploadsFolder = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/images"
                );

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() +
                               Path.GetExtension(image.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                imagePath = "/images/" + fileName;
            }

            var post = new Post
            {
                Content = content,
                ImageUrl = imagePath,
                UserId = userId!,
                CreatedAt = GetEgyptTime()
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(post);
        }

        // ==========================
        // ?? Get All Posts
        // ==========================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var posts = await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PostResponseDto
                {
                    Id = p.Id,
                    Content = p.Content,
                    ImageUrl = p.ImageUrl,
                    UserName = p.User.FullName,
                    UserAvatar = p.User.AvatarUrl ?? "/images/default-user.png",
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt,
                    IsEdited = p.UpdatedAt.HasValue,
                    IsOwner = p.UserId == currentUserId,
                    IsSaved = _context.SavedItems.Any(s => s.UserId == currentUserId && s.ContentType == "post" && s.ItemId == p.Id),
                    IsLiked = _context.Likes.Any(l => l.PostId == p.Id && l.UserId == currentUserId),
                    LikesCount = _context.Likes.Count(l => l.PostId == p.Id),
                    CommentsCount = _context.Comments.Count(c => c.PostId == p.Id)
                })
                .ToListAsync();

            return Ok(posts);
        }

        // ==========================
        // ?? Edit Post
        // ==========================
        [HttpPut("{id}")]
        public async Task<IActionResult> Edit(int id, [FromForm] string? newContent, IFormFile? newImage)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound("Post not found.");

            if (post.UserId != userId)
                return StatusCode(403, "You do not have permission to edit this post.");

            if (string.IsNullOrWhiteSpace(newContent) && newImage == null)
                return BadRequest("Post content or image must be provided.");

            if (!string.IsNullOrWhiteSpace(newContent))
                post.Content = newContent;

            if (newImage != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(post.ImageUrl))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", post.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                // Save new image
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(newImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await newImage.CopyToAsync(stream);
                }
                post.ImageUrl = "/images/" + fileName;
            }

            post.UpdatedAt = GetEgyptTime();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Post updated successfully",
                content = post.Content,
                imageUrl = post.ImageUrl,
                updatedAt = post.UpdatedAt
            });
        }

        // ==========================
        // ??? Delete Post
        // ==========================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var post = await _context.Posts.FindAsync(id);

            if (post == null)
                return NotFound();

            if (post.UserId != userId)
                return Forbid();

            _context.Posts.Remove(post);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Deleted Successfully"
            });
        }

        // ==========================
        // ?? Save Post
        // ==========================
        [HttpPost("{postId}/save")]
        public async Task<IActionResult> SavePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FindAsync(postId);

            if (post == null) return NotFound("Post not found.");

            var exists = await _context.SavedItems.AnyAsync(s => s.UserId == userId && s.ContentType == "post" && s.ItemId == postId);
            if (exists) return BadRequest("Post is already saved.");

            var savedItem = new SavedItem
            {
                UserId = userId!,
                ContentType = "post",
                ItemId = postId,
                SavedAt = GetEgyptTime()
            };

            _context.SavedItems.Add(savedItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post saved successfully." });
        }

        // ==========================
        // ?? Unsave Post
        // ==========================
        [HttpDelete("{postId}/unsave")]
        public async Task<IActionResult> UnsavePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var savedItem = await _context.SavedItems.FirstOrDefaultAsync(s => s.UserId == userId && s.ContentType == "post" && s.ItemId == postId);
            if (savedItem == null) return NotFound("Post is not saved.");

            _context.SavedItems.Remove(savedItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Post unsaved successfully." });
        }

        // ==========================
        // ?? Like Post
        // ==========================
        [HttpPost("{postId}/like")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var post = await _context.Posts.FindAsync(postId);

            if (post == null) return NotFound("Post not found.");

            var exists = await _context.Likes.AnyAsync(l => l.UserId == userId && l.PostId == postId);
            if (exists) return BadRequest("Post is already liked.");

            var like = new Like
            {
                UserId = userId!,
                PostId = postId,
                CreatedAt = GetEgyptTime()
            };

            _context.Likes.Add(like);

            if (post.UserId != userId)
            {
                var notification = new Notification
                {
                    UserId = post.UserId,
                    Title = "New Like",
                    Body = "Someone liked your post.",
                    Type = "Like",
                    CreatedAt = GetEgyptTime()
                };
                _context.Notifications.Add(notification);

                await _context.SaveChangesAsync();

                await _notificationHub.Clients.Group($"user_{post.UserId}")
                    .SendAsync("NotificationCreated", notification);
                
                await _notificationHub.Clients.Group($"user_{post.UserId}")
                    .SendAsync("PostLiked", new { PostId = postId, LikedBy = userId });
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var newLikesCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            await _notificationHub.Clients.All.SendAsync("LikeCountUpdated", new { PostId = postId, LikesCount = newLikesCount });

            return Ok(new { message = "Post liked successfully.", likesCount = newLikesCount });
        }

        // ==========================
        // ?? Unlike Post
        // ==========================
        [HttpDelete("{postId}/unlike")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var like = await _context.Likes.FirstOrDefaultAsync(l => l.UserId == userId && l.PostId == postId);
            if (like == null) return NotFound("Post is not liked.");

            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();

            var newLikesCount = await _context.Likes.CountAsync(l => l.PostId == postId);

            await _notificationHub.Clients.All.SendAsync("LikeCountUpdated", new { PostId = postId, LikesCount = newLikesCount });

            return Ok(new { message = "Post unliked successfully.", likesCount = newLikesCount });
        }

        // ==========================
        // ?? Get Comments
        // ==========================
        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetComments(int postId)
        {
            var comments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .OrderBy(c => c.CreatedAt)
                .Select(c => new
                {
                    c.Id,
                    c.Content,

                    UserName = c.User != null
                        ? c.User.FullName
                        : "Unknown User",

                    UserAvatar =
                        (c.User != null && c.User.AvatarUrl != null)
                        ? c.User.AvatarUrl
                        : "/images/default-user.png",

                    c.CreatedAt,
                    c.UserId
                })
                .ToListAsync();

            return Ok(comments);
        }

        // ==========================
        // ? Add Comment
        // ==========================
        [HttpPost("{postId}/comments")]
        public async Task<IActionResult> AddComment(int postId, [FromBody] CommentDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var postExists = await _context.Posts
                .AnyAsync(p => p.Id == postId);

            if (!postExists)
                return NotFound("Post not found");

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId!,
                Content = dto.Text,
                CreatedAt = GetEgyptTime()
            };

            _context.Comments.Add(comment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment added!"
            });
        }
        // ==========================
        // ?? Edit Comment
        // ==========================
        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> EditComment(
            int commentId,
            [FromBody] string newContent
        )
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = await _context.Comments
                .FindAsync(commentId);

            if (comment == null)
                return NotFound();

            // ???? ??????? ???
            if (comment.UserId != userId)
                return Forbid();

            comment.Content = newContent;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment updated successfully",
                comment.Id,
                comment.Content,
                comment.CreatedAt
            });
        }
        // ==========================
        // ? Delete Comment
        // ==========================
        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = await _context.Comments
                .FindAsync(commentId);

            if (comment == null)
                return NotFound();

            if (comment.UserId != userId)
                return Forbid();

            _context.Comments.Remove(comment);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Comment deleted"
            });
        }
    }
}