using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MedicalApp.API.Data;
using MedicalApp.API.Models;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(Comment model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            model.UserId = userId!;
            model.CreatedAt = DateTime.Now;

            _context.Comments.Add(model);
            await _context.SaveChangesAsync();

            return Ok(model);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _context.Comments.FindAsync(id);
            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return Ok("Deleted");
        }
    }
}
