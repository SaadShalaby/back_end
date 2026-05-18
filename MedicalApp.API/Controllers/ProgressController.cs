using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProgressController(AppDbContext context)
        {
            _context = context;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // =============================
        // 1. Weekly Mood
        // =============================
        [HttpGet("mood-weekly")]
        public async Task<IActionResult> GetWeeklyMood()
        {
            var userId = GetUserId();

            var last7Days = DateTime.Now.AddDays(-6);

            var moods = await _context.MoodEntries
                .Where(m => m.UserId == userId && m.Date >= last7Days)
                .OrderBy(m => m.Date)
                .ToListAsync();

            var result = moods.Select(m => new MoodDto
            {
                Day = m.Date.ToString("ddd")[0].ToString(), // S M T
                Value = m.Value
            });

            return Ok(result);
        }

        // =============================
        // 2. Latest Assessment
        // =============================
        [HttpGet("latest-assessment")]
        public async Task<IActionResult> GetLatestAssessment()
        {
            var userId = GetUserId();

            var assessment = await _context.AssessmentResults
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (assessment == null)
                return NotFound();

            return Ok(new AssessmentDto
            {
                AssessmentName = assessment.AssessmentName,
                Percentage = assessment.Percentage,
                SymptomLevel = assessment.SymptomLevel
            });
        }

        // =============================
        // 3. Assessment Details
        // =============================
        [HttpGet("assessment-details")]
        public async Task<IActionResult> GetAssessmentDetails()
        {
            var userId = GetUserId();

            var assessment = await _context.AssessmentResults
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefaultAsync();

            if (assessment == null)
                return NotFound();

            return Ok(new
            {
                answers = assessment.AnswersJson,
                score = assessment.Percentage,
                recommendation = assessment.Recommendation
            });
        }

        // =============================
        // 4. Save Mood
        // =============================
        [HttpPost("mood")]
        public async Task<IActionResult> SaveMood([FromBody] MoodDto dto)
        {
            var userId = GetUserId();

            if (userId == null)
                return Unauthorized();

            var mood = new MoodEntry
            {
                UserId = userId,
                Value = dto.Value,
                Date = DateTime.Now
            };

            _context.MoodEntries.Add(mood);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Mood saved successfully"
            });
        }
    }
}