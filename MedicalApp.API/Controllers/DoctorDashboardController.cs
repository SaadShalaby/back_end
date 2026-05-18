using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Doctor")]
    public class DoctorDashboardController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorDashboardController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetDoctorId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 0️⃣ معلومات الطبيب (Header)
        [HttpGet("header")]
        public async Task<IActionResult> GetDoctorHeader()
        {
            var doctorId = GetDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == doctorId);

            if (doctor == null) return NotFound("Doctor not found.");

            return Ok(new
            {
                name = doctor.Name,
                specialization = doctor.Specialization,
                experienceYears = doctor.ExperienceYears,
                imageUrl = doctor.User?.AvatarUrl ?? doctor.ImageUrl ?? "https://api.dicebear.com/7.x/avataaars/svg?seed=doctor"
            });
        }

        // 1️⃣ الإحصائيات
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var doctorId = GetDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            try
            {
                var stats = new
                {
                    Sessions = await _context.DoctorSessions.CountAsync(s => s.DoctorId == doctorId),
                    News = 3,
                    Patients = await _context.DoctorSessions
                        .Where(s => s.DoctorId == doctorId && s.PatientId != null)
                        .Select(s => s.PatientId)
                        .Distinct()
                        .CountAsync(),
                    Upcoming = await _context.DoctorSessions
                        .CountAsync(s => s.DoctorId == doctorId && s.ScheduledAt > DateTime.Now),
                    Records = await _context.PatientRecords.CountAsync(r => r.DoctorId == doctorId),
                    Reports = await _context.Reports.CountAsync(r => r.DoctorId == doctorId)
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // 2️⃣ النشاطات الأخيرة
        [HttpGet("recent-activity")]
        public async Task<IActionResult> GetRecentActivity()
        {
            var activities = new List<object>
            {
                new { title = "Patient Sara Hany Completed", subtitle = "pre-session", time = "15 min ago", type = "check" },
                new { title = "New message from Patient Omar Saad", subtitle = "pre-session", time = "19 min ago", type = "message" },
                new { title = "Patient Amr Khaled scheduled a new session", subtitle = "new session", time = "2 hours ago", type = "calendar" }
            };

            return Ok(activities);
        }

        // 3️⃣ قائمة المرضى
        [HttpGet("my-patients")]
        public async Task<IActionResult> GetMyPatients()
        {
            var doctorId = GetDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            var patientIds = await _context.DoctorSessions
                .Where(s => s.DoctorId == doctorId && s.PatientId != null)
                .Select(s => s.PatientId)
                .Distinct()
                .ToListAsync();

            var patients = await _context.Users
                .Where(u => patientIds.Contains(u.Id))
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    avatarUrl = u.AvatarUrl ?? "https://api.dicebear.com/7.x/avataaars/svg?seed=default"
                })
                .ToListAsync();

            return Ok(patients);
        }

        // 4️⃣ إضافة سجل طبي
        [HttpPost("add-record")]
        public async Task<IActionResult> AddRecord([FromBody] RecordDto dto)
        {
            var doctorId = GetDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            var record = new PatientRecord
            {
                DoctorId = doctorId,
                PatientId = dto.PatientId,  // ✅ تم التصحيح ليكون string في الاثنين
                Diagnosis = dto.Diagnosis,
                Notes = dto.Notes,
                TreatmentPlan = dto.TreatmentPlan,
                CreatedAt = DateTime.Now
            };

            _context.PatientRecords.Add(record);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Patient record saved successfully!" });
        }

        // 5️⃣ التقارير الطبية
        [HttpGet("medical-reports")]
        public async Task<IActionResult> GetMedicalReports()
        {
            var doctorId = GetDoctorId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            var reports = await _context.Reports
                .Where(r => r.DoctorId == doctorId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

            return Ok(reports);
        }
    }
}