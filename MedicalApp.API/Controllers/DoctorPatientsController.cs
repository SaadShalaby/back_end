//using MedicalApp.API.Data;
//using MedicalApp.API.DTOs;
//using MedicalApp.API.Models;
//using MedicalApp.API.Services.Interfaces;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;

//namespace MedicalApp.API.Controllers
//{
//    [ApiController]
//    [Route("api/[controller]")]
//    public class DoctorPatientsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly UserManager<ApplicationUser> _userManager;


//        public DoctorPatientsController(AppDbContext context, UserManager<ApplicationUser> userManager)
//        {
//            _context = context;
//            _userManager = userManager;
//        }

//        // --- 🟢 عرض كل المرضى (مع الصور والبحث) ---
//        [HttpGet("patients")]
//        public async Task<IActionResult> GetAllPatients([FromQuery] string? search)
//        {
//            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//            var patientsQuery = _context.DoctorSessions
//                .Where(s => s.DoctorId == doctorId)
//                .Include(s => s.Patient)
//                    .ThenInclude(p => p.User)
//                .Select(s => s.Patient)
//                .Distinct(); // عشان لو المريض عنده أكتر من session

//            if (!string.IsNullOrEmpty(search))
//            {
//                patientsQuery = patientsQuery
//                    .Where(p => p.User.FullName.Contains(search));
//            }

//            var patients = await patientsQuery
//                .Select(p => new
//                {
//                    p.Id,
//                    Name = p.User.FullName,
//                    Image = p.User.AvatarUrl,
//                    p.Age,
//                    p.Gender
//                })
//                .ToListAsync();

//            return Ok(patients);
//        }

//        // --- 🔵 البحث عن الدكاترة (عشان شاشة الـ Flutter اللي كانت فاضية) ---
//        [HttpGet("search-doctors")]
//        [AllowAnonymous] // مسموح لأي حد يبحث عن دكتور
//        public async Task<IActionResult> SearchDoctors([FromQuery] string? query, [FromQuery] string? specialization)
//        {
//            var doctorQuery = _context.Doctors.Include(d => d.User).AsQueryable();

//            if (!string.IsNullOrEmpty(query))
//                doctorQuery = doctorQuery.Where(d => d.User.FullName.Contains(query) || d.Specialization.Contains(query));

//            if (!string.IsNullOrEmpty(specialization))
//                doctorQuery = doctorQuery.Where(d => d.Specialization == specialization);

//            var doctors = await doctorQuery.ToListAsync();

//            var result = doctors.Select(d => new {
//                d.Id,
//                d.Name,
//                d.Specialization,
//                d.ExperienceYears,
//                d.Rating,
//                ImageUrl = d.User.AvatarUrl ?? "default-doctor.png",
//                d.Bio
//            });

//            return Ok(result);
//        }

//        // --- 🟡 بروفايل المريض بالتفصيل ---
//        [HttpGet("patient/{id}")]
//        public async Task<IActionResult> GetPatientProfile(int id)
//        {
//            var patient = await _context.Patients
//                .Include(p => p.User)
//                .FirstOrDefaultAsync(p => p.Id == id);

//            if (patient == null) return NotFound();

//            return Ok(new
//            {
//                patient.Id,
//                Name = patient.User.FullName,
//                patient.User.Email,
//                patient.Age,
//                patient.Gender,
//                ImageUrl = patient.User.AvatarUrl,
//                JoinedDate = patient.RegisteredAt
//            });
//        }
//    }
//}
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
    public class DoctorPatientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorPatientsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager
        )
        {
            _context = context;
            _userManager = userManager;
        }

        // ==========================
        // 🟢 عرض كل المرضى للدكتور
        // ==========================
        [HttpGet("patients")]
        [Authorize]
        public async Task<IActionResult> GetAllPatients(
            [FromQuery] string? search
        )
        {
            var doctorId = User.FindFirstValue(
                ClaimTypes.NameIdentifier
            );

            if (string.IsNullOrEmpty(doctorId))
                return Unauthorized();

            // نجيب كل المرضى المرتبطين بالدكتور من الـ Sessions
            var patientUserIds = await _context.DoctorSessions
                .Where(s => s.DoctorId == doctorId)
                .Select(s => s.PatientId)
                .Distinct()
                .ToListAsync();

            // نجيب بيانات المرضى
            var patientsQuery = _context.Patients
                .Include(p => p.User)
                .Where(p => patientUserIds.Contains(p.UserId));

            // البحث بالاسم
            if (!string.IsNullOrEmpty(search))
            {
                patientsQuery = patientsQuery
                    .Where(p =>
                        p.User.FullName.Contains(search)
                    );
            }

            var patients = await patientsQuery
                .Select(p => new
                {
                    p.Id,
                    Name = p.User.FullName,
                    Image = p.User.AvatarUrl,
                    p.Age,
                    p.Gender,
                    JoinedAt = p.RegisteredAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        // ==========================
        // 🔵 البحث عن الدكاترة
        // ==========================
        [HttpGet("search-doctors")]
        [AllowAnonymous]
        public async Task<IActionResult> SearchDoctors(
            [FromQuery] string? query,
            [FromQuery] string? specialization
        )
        {
            var doctorQuery = _context.Doctors
                .Include(d => d.User)
                .AsQueryable();

            // البحث بالاسم أو التخصص
            if (!string.IsNullOrEmpty(query))
            {
                doctorQuery = doctorQuery.Where(d =>
                    d.User.FullName.Contains(query) ||
                    d.Specialization.Contains(query)
                );
            }

            // فلترة بالتخصص
            if (!string.IsNullOrEmpty(specialization))
            {
                doctorQuery = doctorQuery.Where(d =>
                    d.Specialization == specialization
                );
            }

            var doctors = await doctorQuery.ToListAsync();

            var result = doctors.Select(d => new
            {
                d.Id,
                d.Name,
                d.Specialization,
                d.ExperienceYears,
                d.Rating,
                ImageUrl = d.User.AvatarUrl ?? "default-doctor.png",
                d.Bio
            });

            return Ok(result);
        }

        // ==========================
        // 🟡 بروفايل المريض
        // ==========================
        [HttpGet("patient/{id}")]
        [Authorize]
        public async Task<IActionResult> GetPatientProfile(
            int id
        )
        {
            var patient = await _context.Patients
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (patient == null)
                return NotFound(new
                {
                    message = "المريض غير موجود"
                });

            return Ok(new
            {
                patient.Id,
                Name = patient.User.FullName,
                patient.User.Email,
                patient.Age,
                patient.Gender,
                ImageUrl = patient.User.AvatarUrl,
                JoinedDate = patient.RegisteredAt
            });
        }
    }
}