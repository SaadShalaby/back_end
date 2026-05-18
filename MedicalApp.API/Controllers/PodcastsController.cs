using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class PodcastsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PodcastsController(AppDbContext context)
    {
        _context = context;
    }

    // ==============================
    // 1️⃣ Get All Published Episodes
    // ==============================
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var episodes = await _context.PodcastEpisodes
            .Where(p => p.IsPublished)
            .OrderByDescending(p => p.PublishDate)
            .Select(p => new PodcastListDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description,
                AudioUrl = p.AudioUrl,
                CoverImageUrl = p.CoverImageUrl,
                DurationInSeconds = p.DurationInSeconds,
                PublishDate = p.PublishDate
            })
            .ToListAsync();

        return Ok(episodes);
    }

    // ==============================
    // 2️⃣ Get Single Episode Details
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
                AudioUrl = p.AudioUrl,
                CoverImageUrl = p.CoverImageUrl,
                DurationInSeconds = p.DurationInSeconds,
                PublishDate = p.PublishDate
            })
            .FirstOrDefaultAsync();

        if (episode == null)
            return NotFound();

        return Ok(episode);
    }
}