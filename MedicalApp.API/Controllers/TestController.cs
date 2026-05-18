using MedicalApp.API.Data;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public TestController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("seed-doctor")]
        public async Task<IActionResult> SeedDoctor()
        {
            // 1. إنشاء مستخدم جديد للدكتور
            var doctorEmail = "testdoctor@example.com";
            var user = await _userManager.FindByEmailAsync(doctorEmail);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = doctorEmail,
                    Email = doctorEmail,
                    FullName = "دكتور أحمد محمد (تجريبي)",
                    AvatarUrl = "https://api.dicebear.com/7.x/avataaars/svg?seed=Ahmed",
                    EmailConfirmed = true
                };
                var result = await _userManager.CreateAsync(user, "Password123!");
                if (!result.Succeeded) return BadRequest(result.Errors);
                
                // إضافة رول الدكتور
                await _userManager.AddToRoleAsync(user, "Doctor");
            }

            // 2. إنشاء سجل في جدول الدكاترة
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null)
            {
                doctor = new Doctor
                {
                    UserId = user.Id,
                    Name = user.FullName,
                    Specialization = "أخصائي نفسي",
                    ExperienceYears = 10,
                    Bio = "هذا دكتور تجريبي للتأكد من عمل النظام بشكل صحيح.",
                    Rating = 4.8,
                    SessionPrice = 300
                };
                _context.Doctors.Add(doctor);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Test Doctor Seeded Successfully!", email = doctorEmail, password = "Password123!" });
        }
    }
}