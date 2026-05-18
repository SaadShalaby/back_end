using MedicalApp.API.Data;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public AdminDashboardController(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [HttpGet("pending-sessions")]
        public async Task<IActionResult> GetPendingSessions()
        {
            var sessions = await _context.DoctorSessions
                .Where(s => s.Status == "Pending")
                .Include(s => s.Patient)
                .ThenInclude(p => p.User)
                .Select(s => new
                {
                    s.Id,
                    s.DoctorId,
                    s.PatientId,
                    PatientName = s.Patient.User.FullName,
                    s.SessionType,
                    s.ScheduledAt,
                    s.Status
                })
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost("accept-session/{id}")]
        public async Task<IActionResult> AcceptSession(int id)
        {
            var session = await _context.DoctorSessions.FindAsync(id);
            if (session == null) return NotFound(new { message = "Session not found." });

            if (session.Status != "Pending")
                return BadRequest(new { message = "Only pending sessions can be accepted." });

            session.Status = "Accepted";
            await _context.SaveChangesAsync();

            // إشعار للمريض
            await _notificationService.SendNotificationAsync(
                targetUserId: session.PatientId,
                title: "تم قبول جلستك ✅",
                body: $"تم تأكيد حجز الجلسة الخاص بك بتاريخ {session.ScheduledAt:yyyy-MM-dd HH:mm} من قبل الإدارة.",
                type: "Booking"
            );
            
            // إشعار للدكتور
            await _notificationService.SendNotificationAsync(
                targetUserId: session.DoctorId,
                title: "تمت الموافقة على جلسة 📅",
                body: $"تم تأكيد جلسة جديدة لك مع المريض {session.PatientName} بتاريخ {session.ScheduledAt:yyyy-MM-dd HH:mm}.",
                type: "Booking"
            );

            return Ok(new { message = "تم قبول الجلسة بنجاح" });
        }

        [HttpPost("reject-session/{id}")]
        public async Task<IActionResult> RejectSession(int id)
        {
            var session = await _context.DoctorSessions.FindAsync(id);
            if (session == null) return NotFound(new { message = "Session not found." });

            if (session.Status != "Pending")
                return BadRequest(new { message = "Only pending sessions can be rejected." });

            session.Status = "Rejected";
            await _context.SaveChangesAsync();

            // إشعار للمريض
            await _notificationService.SendNotificationAsync(
                targetUserId: session.PatientId,
                title: "تم رفض الجلسة ❌",
                body: $"نأسف، تم رفض طلب الجلسة الخاص بك بتاريخ {session.ScheduledAt:yyyy-MM-dd HH:mm}.",
                type: "Booking"
            );

            return Ok(new { message = "تم رفض الجلسة بنجاح" });
        }
    }
}
