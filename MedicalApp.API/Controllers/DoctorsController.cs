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
    [Authorize]
    public class DoctorsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorsController(AppDbContext context)
        {
            _context = context;
        }

        // 1. عرض كل الدكاترة (للشاشة الرئيسية)
        [HttpGet]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    Name = d.Name,
                    Specialization = d.Specialization,
                    ExperienceYears = d.ExperienceYears,
                    Rating = d.Rating,
                    ImageUrl = d.User.AvatarUrl,
                    Bio = d.Bio,
                    SessionPrice = d.SessionPrice
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // 2. البحث عن دكتور (بالاسم أو التخصص)
        [HttpGet("search")]
        public async Task<IActionResult> SearchDoctor([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Please provide a search term.");

            var searchTerm = query.Trim().ToLower();

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Where(d => d.Name.Contains(searchTerm) || 
                            d.Specialization.Contains(searchTerm) || 
                            (d.Bio != null && d.Bio.Contains(searchTerm)))
                .Select(d => new DoctorDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    Name = d.Name,
                    Specialization = d.Specialization,
                    ExperienceYears = d.ExperienceYears,
                    Rating = d.Rating,
                    ImageUrl = d.User.AvatarUrl,
                    Bio = d.Bio,
                    SessionPrice = d.SessionPrice
                })
                .ToListAsync();

            return Ok(doctors);
        }

        // --- 🆕 3. تعديل بيانات بروفايل الدكتور (التعديل الجديد) ---
        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] DoctorUpdateDto dto)
        {
            // سحب الـ ID بتاع الدكتور من الـ Token
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // نجيب بيانات الدكتور واليوزر المرتبط بيه
            var doctor = await _context.Doctors
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (doctor == null) return NotFound("Doctor profile not found");

            // تحديث بيانات جدول الدكتور
            doctor.Name = dto.FullName;
            doctor.Specialization = dto.Specialization;
            doctor.ExperienceYears = dto.ExperienceYears;
            
            if (dto.Bio != null) doctor.Bio = dto.Bio;
            if (dto.SessionPrice.HasValue) doctor.SessionPrice = dto.SessionPrice.Value;

            // تحديث الاسم في جدول الـ Users الأساسي عشان يسمع في الأكونت
            if (doctor.User != null)
            {
                doctor.User.FullName = dto.FullName;
            }

            // تأكيد التعديل على مستوى الـ Entry لضمان التخزين في الداتابيز
            _context.Entry(doctor).State = EntityState.Modified;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully",
                newName = doctor.Name,
                newSpec = doctor.Specialization
            });
        }

        // 4. حجز جلسة (الزرار الأزرق Book Session)
        [HttpPost("book-session")]
        public async Task<IActionResult> BookSession(BookSessionDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(patientId))
                return Unauthorized("User context not found.");

            var session = new DoctorSession
            {
                DoctorId = dto.DoctorId.ToString(),
                PatientId = patientId,
                ScheduledAt = dto.SessionDate,
                SessionType = dto.SessionType,
                IsStarted = false
            };

            _context.DoctorSessions.Add(session);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Session booked successfully", sessionId = session.Id });
        }
    }
}