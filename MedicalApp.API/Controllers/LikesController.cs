using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedicalApp.API.Data;
using MedicalApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LikesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("{postId}")]
        public async Task<IActionResult> ToggleLike(int postId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. نتأكد الأول البوست موجود ولا لا
            var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
            if (!postExists) return NotFound("Post not found");

            // 2. نشوف هل المستخدم ده عامل لايك قبل كدة ولا لا
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                // لو عامل لايك قبل كدة، نمسحه (Unlike)
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Unliked", isLiked = false });
            }

            var like = new Like
            {
                PostId = postId,
                UserId = userId!
            };

            _context.Likes.Add(like);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Liked", isLiked = true });
        }
    }
}