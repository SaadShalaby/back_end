using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public NotificationsController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // =============================
        // 1. Get All Notifications
        // =============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    Body = n.Body,
                    Type = n.Type,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                })
                .ToListAsync();

            return Ok(notifications);
        }

        // =============================
        // 2. Unread Count
        // =============================
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetUserId();

            var count = await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);

            return Ok(new { count });
        }

        // =============================
        // 3. Mark Single Read
        // =============================
        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = GetUserId();

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        // =============================
        // 4. Clear All
        // =============================
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearAll()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .ToListAsync();

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // =============================
        // 5. Mark All Read
        // =============================
        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetUserId();

            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in notifications)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}