using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PodcastsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public PodcastsController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // ==============================
        // 1️⃣ Get All Published Episodes (Public/Authenticated)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var episodes = await _context.PodcastEpisodes
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PodcastListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FileUrl = p.FileUrl,
                    FileName = p.FileName,
                    MimeType = p.MimeType,
                    FileSize = p.FileSize,
                    CoverImageUrl = p.CoverImageUrl,
                    Duration = p.Duration,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();

            return Ok(episodes);
        }

        // ==============================
        // 2️⃣ Get Single Episode Details (Public/Authenticated)
        // ==============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var episode = await _context.PodcastEpisodes
                .Where(p => p.Id == id && p.IsPublished)
                .Select(p => new PodcastListDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    FileUrl = p.FileUrl,
                    FileName = p.FileName,
                    MimeType = p.MimeType,
                    FileSize = p.FileSize,
                    CoverImageUrl = p.CoverImageUrl,
                    Duration = p.Duration,
                    CreatedAt = p.CreatedAt
                })
                .FirstOrDefaultAsync();

            if (episode == null)
                return NotFound();

            return Ok(episode);
        }

        // ==============================
        // 3️⃣ Upload Podcast (Admin Only)
        // ==============================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreatePodcastDto dto)
        {
            string fileUrl = await _fileStorageService.SaveFileAsync(dto.File, "uploads/podcasts");

            var podcast = new PodcastEpisode
            {
                Title = dto.Title,
                Description = dto.Description,
                FileUrl = fileUrl,
                FileName = dto.File.FileName,
                MimeType = dto.File.ContentType,
                FileSize = dto.File.Length,
                CoverImageUrl = dto.CoverImageUrl,
                Duration = dto.Duration,
                CreatedAt = DateTime.Now,
                IsPublished = true
            };

            _context.PodcastEpisodes.Add(podcast);
            await _context.SaveChangesAsync();

            return Ok(podcast);
        }

        // ==============================
        // 4️⃣ Delete Podcast (Admin Only)
        // ==============================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var podcast = await _context.PodcastEpisodes.FindAsync(id);
            if (podcast == null) return NotFound(new { message = "Podcast not found." });

            // Delete physical file
            await _fileStorageService.DeleteFileAsync(podcast.FileUrl);

            _context.PodcastEpisodes.Remove(podcast);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Podcast deleted successfully." });
        }
    }
}