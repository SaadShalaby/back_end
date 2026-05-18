using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Patient")]
    public class BookSessionController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;

        public BookSessionController(AppDbContext context, INotificationService notificationService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        /// <summary>
        /// المريض يحجز جلسة عند دكتور معين — يوصل للدكتور إشعار فوري
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> BookSession([FromBody] BookSessionDto dto)
        {
            var patientUserId = GetUserId();
            if (string.IsNullOrEmpty(patientUserId))
                return Unauthorized();

            // جلب بيانات المريض عشان نعرف اسمه
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == patientUserId);

            if (patient == null)
                return NotFound(new { message = "بيانات المريض غير موجودة." });

            // جلب بيانات الدكتور عشان نجيب الـ UserId بتاعه ونبعتله الإشعار
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

            if (doctor == null)
                return NotFound(new { message = "الدكتور غير موجود." });

            // إنشاء الجلسة المحجوزة
            var session = new DoctorSession
            {
                DoctorId = doctor.UserId,
                PatientId = patientUserId,
                PatientName = patient.User.FullName,
                SessionType = dto.SessionType?.ToLower() ?? "chat",
                ScheduledAt = dto.SessionDate,
                Price = 0,
                IsStarted = false
            };

            _context.DoctorSessions.Add(session);
            await _context.SaveChangesAsync();

            // ✅ إشعار real-time للدكتور: مريض حجز جلسة
            await _notificationService.SendNotificationAsync(
                targetUserId: doctor.UserId,
                title: "حجز جلسة جديد 📅",
                body: $"قام {patient.User.FullName} بحجز جلسة بتاريخ {session.ScheduledAt:yyyy-MM-dd HH:mm}",
                type: "Booking"
            );

            // ✅ إشعار للمريض نفسه لتأكيد الحجز
            await _notificationService.SendNotificationAsync(
                targetUserId: patientUserId,
                title: "تأكيد الحجز ✅",
                body: $"تم تسجيل حجزك مع د. {doctor.Name} بنجاح ليوم {session.ScheduledAt:yyyy-MM-dd} (قيد الانتظار)",
                type: "Booking"
            );

            // ✅ إشعار للادمن 
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            foreach (var admin in admins)
            {
                await _notificationService.SendNotificationAsync(
                    targetUserId: admin.Id,
                    title: "طلب حجز جديد 📅",
                    body: $"قام المريض {patient.User.FullName} بطلب حجز جلسة مع د. {doctor.Name} بتاريخ {session.ScheduledAt:yyyy-MM-dd HH:mm}",
                    type: "Booking"
                );
            }

            return Ok(new { message = "تم حجز الجلسة بنجاح", sessionId = session.Id });
        }
    }
}
