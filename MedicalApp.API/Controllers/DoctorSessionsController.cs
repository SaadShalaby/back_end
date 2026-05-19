using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DoctorSessionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;
        private readonly INotificationService _notificationService;

        public DoctorSessionsController(
            AppDbContext context,
            IFileStorageService fileStorageService,
            INotificationService notificationService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
            _notificationService = notificationService;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. إضافة جلسة جديدة لمريض معين
        [HttpPost("{patientId}")]
        public async Task<IActionResult> AddSession(int patientId, [FromForm] CreateSessionDto dto)
        {
            var doctorId = GetUserId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            // لازم نستخدم Include عشان نجيب بيانات المريض والـ User بتاعه
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == patientId);

            if (patient == null) return NotFound(new { message = "المريض غير موجود في النظام." });

            var session = new DoctorSession
            {
                DoctorId = doctorId,
                PatientId = patient.UserId, // الربط بالـ UserId النصي
                PatientName = patient.User.FullName, // سحب الاسم من الـ User
                SessionType = dto.SessionType.ToLower(),
                ScheduledAt = dto.ScheduledAt,
                Price = dto.Price ?? 0,
                IsStarted = false
            };



            // رفع الملف لو موجود
            if (dto.MediaFile != null)
            {
                var fileUrl = await _fileStorageService.SaveFileAsync(dto.MediaFile, "uploads/sessions");
                
                session.SessionMediaUrl = fileUrl;

                // بنحط اللينك في الخانة المناسبة حسب نوع الجلسة (للتوافق القديم)
                switch (session.SessionType)
                {
                    case "video":
                        session.VideoUrl = fileUrl;
                        break;
                    case "audio":
                        session.AudioUrl = fileUrl;
                        break;
                    case "pdf":
                        session.PdfUrl = fileUrl;
                        break;
                    case "chat":
                    default:
                        session.ImageUrl = fileUrl;
                        break;
                }
            }
            else
            {
                // لو مفيش ملف مرفوع، نستخدم اللينكات لو مبعوتة كـ strings (اختياري)
                session.VideoUrl = dto.VideoUrl;
                session.AudioUrl = dto.AudioUrl;
                session.PdfUrl = dto.PdfUrl;
                session.ImageUrl = dto.ImageUrl;
                
                if (!string.IsNullOrEmpty(dto.VideoUrl)) session.SessionMediaUrl = dto.VideoUrl;
                else if (!string.IsNullOrEmpty(dto.AudioUrl)) session.SessionMediaUrl = dto.AudioUrl;
                else if (!string.IsNullOrEmpty(dto.PdfUrl)) session.SessionMediaUrl = dto.PdfUrl;
                else if (!string.IsNullOrEmpty(dto.ImageUrl)) session.SessionMediaUrl = dto.ImageUrl;
            }

            _context.DoctorSessions.Add(session);
            await _context.SaveChangesAsync();

            // ✅ إشعار real-time للمريض: الدكتور رفع جلسة جديدة
            await _notificationService.SendNotificationAsync(
                targetUserId: patient.UserId,
                title: "جلسة جديدة متاحة 🎯",
                body: $"قام الدكتور بإضافة جلسة جديدة لك بتاريخ {session.ScheduledAt:yyyy-MM-dd}",
                type: "Session"
            );

            return Ok(new { message = "تمت إضافة الجلسة بنجاح" });

        }


        // 2. عرض كل الجلسات (بناءً على اليوزر دكتور ولا مريض)
        [HttpGet]
        public async Task<IActionResult> GetAllSessions()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var sessions = await _context.DoctorSessions
                .Where(s => s.DoctorId == userId || s.PatientId == userId)
                .OrderByDescending(s => s.ScheduledAt)
                .ToListAsync();

            return Ok(sessions);
        }

        // 3. الـ Podcast (بيجيب المحتوى العام فقط وليس الجلسات الخاصة)
        [HttpGet("podcasts")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPodcasts()
        {
            var podcasts = await _context.PodcastEpisodes
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishDate)
                .Select(p => new {
                    p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    AudioUrl = p.AudioUrl,
                    ImageUrl = p.CoverImageUrl,
                    DurationSeconds = p.DurationInSeconds,
                    PublishedAt = p.PublishDate
                })
                .ToListAsync();

            return Ok(podcasts);
        }

        // 4. الـ Resources (تم التعديل لتدعم البحث وتعمل للمريض والدكتور تلقائياً)
        [HttpGet("resources")]
        public async Task<IActionResult> GetResources([FromQuery] string? search = null)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var query = _context.DoctorSessions
                .Where(s => (s.PatientId == userId || s.DoctorId == userId) &&
                           (!string.IsNullOrEmpty(s.VideoUrl) || !string.IsNullOrEmpty(s.PdfUrl) || !string.IsNullOrEmpty(s.ImageUrl) || !string.IsNullOrEmpty(s.AudioUrl)));

            var resourcesList = await query
                .OrderByDescending(s => s.ScheduledAt)
                .Select(s => new {
                    s.Id,
                    DoctorName = _context.Doctors.Where(d => d.UserId == s.DoctorId).Select(d => d.Name).FirstOrDefault(),
                    s.PatientName,
                    s.VideoUrl,
                    s.PdfUrl,
                    s.ImageUrl,
                    s.AudioUrl,
                    s.SessionType,
                    s.ScheduledAt
                })
                .ToListAsync();

            // تحضير البيانات بشكل يفهمه الموبايل (title, type)
            var result = resourcesList.Select(r => new {
                r.Id,
                title = r.DoctorName != null ? $"Session with Dr. {r.DoctorName}" : $"Session {r.SessionType}",
                type = !string.IsNullOrEmpty(r.PdfUrl) ? "article" :
                       !string.IsNullOrEmpty(r.VideoUrl) ? "video" :
                       !string.IsNullOrEmpty(r.AudioUrl) ? "audio" : "image",
                r.VideoUrl,
                r.PdfUrl,
                r.ImageUrl,
                r.AudioUrl,
                r.SessionType,
                date = r.ScheduledAt.ToString("yyyy-MM-dd")
            }).ToList();

            // تطبيق السيرش
            if (!string.IsNullOrEmpty(search))
            {
                result = result.Where(r => 
                    r.title.Contains(search, StringComparison.OrdinalIgnoreCase) || 
                    r.type.Contains(search, StringComparison.OrdinalIgnoreCase)
                ).ToList();
            }

            return Ok(result);
        }

        [HttpGet("upcoming")]
        public async Task<IActionResult> GetUpcomingSessions()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var sessions = await _context.DoctorSessions
                .Where(s => (s.DoctorId == userId || s.PatientId == userId) && s.ScheduledAt > DateTime.Now)
                .OrderBy(s => s.ScheduledAt)
                .ToListAsync();

            return Ok(sessions);
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartSession(int id)
        {
            var session = await _context.DoctorSessions.FindAsync(id);
            if (session == null) return NotFound();

            if (session.DoctorId != GetUserId()) return Forbid();

            session.IsStarted = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session started" });
        }
    }
}