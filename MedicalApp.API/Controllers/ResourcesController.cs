using System;
using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using MedicalApp.API.Services.Interfaces;
using ResourceType = MedicalApp.API.Models.Enums.ResourceType;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorageService;

        public ResourcesController(AppDbContext context, IFileStorageService fileStorageService)
        {
            _context = context;
            _fileStorageService = fileStorageService;
        }

        // GET all
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resources = await _context.Resources
                .AsNoTracking()
                .Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    CoverImageUrl = r.CoverImageUrl,
                    Type = r.Type.ToString(),
                    FileUrl = r.FileUrl,
                    FileName = r.FileName,
                    MimeType = r.MimeType,
                    Duration = r.Duration,
                    FileSize = r.FileSize,
                    CreatedDate = r.CreatedDate,
                    IsSaved = currentUserId != null && _context.SavedItems.Any(s => s.UserId == currentUserId && s.ContentType == "resource" && s.ItemId == r.Id)
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return Ok(resources);
        }

        // GET by type
        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(ResourceType type)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var resources = await _context.Resources
                .AsNoTracking()
                .Where(r => r.Type == type)
                .Select(r => new ResourceDto
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    CoverImageUrl = r.CoverImageUrl,
                    Type = r.Type.ToString(),
                    FileUrl = r.FileUrl,
                    FileName = r.FileName,
                    MimeType = r.MimeType,
                    Duration = r.Duration,
                    FileSize = r.FileSize,
                    CreatedDate = r.CreatedDate,
                    IsSaved = currentUserId != null && _context.SavedItems.Any(s => s.UserId == currentUserId && s.ContentType == "resource" && s.ItemId == r.Id)
                })
                .ToListAsync();

            return Ok(resources);
        }

        // POST
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromForm] CreateResourceDto dto)
        {
            string fileUrl = await _fileStorageService.SaveFileAsync(dto.File, "uploads/resources");

            var resource = new Resource
            {
                Title = dto.Title,
                Description = dto.Description,
                CoverImageUrl = dto.CoverImageUrl,
                Type = dto.Type,
                Duration = dto.Duration,
                FileUrl = fileUrl,
                FileName = dto.File.FileName,
                MimeType = dto.File.ContentType,
                FileSize = dto.File.Length
            };

            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();

            return Ok(resource);
        }

        // ==========================
        // ?? Save Resource
        // ==========================
        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<IActionResult> SaveResource(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null) return NotFound("Resource not found.");

            var exists = await _context.SavedItems.AnyAsync(s => s.UserId == userId && s.ContentType == "resource" && s.ItemId == id);
            if (exists) return BadRequest("Resource is already saved.");

            var savedItem = new SavedItem
            {
                UserId = userId!,
                ContentType = "resource",
                ItemId = id,
                SavedAt = DateTime.Now
            };

            _context.SavedItems.Add(savedItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resource saved successfully." });
        }

        // ==========================
        // ?? Unsave Resource
        // ==========================
        [HttpDelete("{id}/unsave")]
        [Authorize]
        public async Task<IActionResult> UnsaveResource(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var resource = await _context.Resources.FindAsync(id);

            if (resource == null) return NotFound("Resource not found.");

            var savedItem = await _context.SavedItems.FirstOrDefaultAsync(s => s.UserId == userId && s.ContentType == "resource" && s.ItemId == id);
            if (savedItem == null) return NotFound("Resource is not saved.");

            _context.SavedItems.Remove(savedItem);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Resource unsaved successfully." });
        }
    }
}
