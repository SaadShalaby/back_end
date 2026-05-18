using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HelpController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HelpController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ========================
        // 1️⃣ FAQs
        // ========================
        [HttpGet("faqs")]
        [AllowAnonymous]
        public async Task<IActionResult> GetFaqs()
        {
            var faqs = await _context.Faqs
                .Where(f => f.IsActive)
                .ToListAsync();

            return Ok(faqs);
        }

        // ========================
        // 2️⃣ Contact Info
        // ========================
        [HttpGet("contact")]
        [AllowAnonymous]
        public IActionResult GetContactInfo()
        {
            return Ok(new
            {
                Email = "support@medicalapp.com",
                Phone = "+201234567890"
            });
        }

        // ========================
        // 3️⃣ Create Support Ticket
        // ========================
        [Authorize]
        [HttpPost("tickets")]
        public async Task<IActionResult> CreateTicket(CreateTicketDto dto)
        {
            var user = await _userManager.GetUserAsync(User);

            var ticket = new SupportTicket
            {
                UserId = user.Id,
                Subject = dto.Subject,
                Message = dto.Message
            };

            _context.SupportTickets.Add(ticket);
            await _context.SaveChangesAsync();

            return Ok(ticket);
        }

        // ========================
        // 4️⃣ Get My Tickets
        // ========================
        [Authorize]
        [HttpGet("tickets")]
        public async Task<IActionResult> GetMyTickets()
        {
            var user = await _userManager.GetUserAsync(User);

            var tickets = await _context.SupportTickets
                .Where(t => t.UserId == user.Id)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tickets);
        }

        // ========================
        // 5️⃣ Send Email Form
        // ========================
        [Authorize]
        [HttpPost("send-email")]
        public async Task<IActionResult> SendEmail(SendEmailDto dto)
        {
            var user = await _userManager.GetUserAsync(User);

            // هنا تقدر تربط SMTP Service بعدين
            // حالياً بس بنرجع OK

            return Ok(new
            {
                message = "Email sent successfully",
                from = user.Email
            });
        }
    }
}
