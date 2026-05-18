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
    public class DoctorReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorReportsController(AppDbContext context)
        {
            _context = context;
        }

        private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpGet]
        public async Task<IActionResult> GetReports()
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var reports = await _context.Reports
                .Where(r => r.DoctorId == userId || r.PatientId == userId)
                .OrderByDescending(r => r.ReportDate)
                .ToListAsync();

            return Ok(reports);
        }

        [HttpPost]
        public async Task<IActionResult> CreateReport([FromBody] CreateReportDto dto)
        {
            var doctorId = GetUserId();
            if (string.IsNullOrEmpty(doctorId)) return Unauthorized();

            var report = new Report
            {
                DoctorId = doctorId,
                PatientId = dto.PatientId,
                Type = dto.Type,
                FileUrl = dto.FileUrl,
                ReportDate = DateTime.Now
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Report created successfully", reportId = report.Id });
        }
    }
}
