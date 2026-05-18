using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SettingsController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public SettingsController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<UserSettingsDto>> GetSettings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var dto = new UserSettingsDto
            {
                FullName = user.FullName,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                NotificationsEnabled = user.NotificationsEnabled,
                Language = user.Language
            };

            return Ok(dto);
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UserSettingsDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.FullName = dto.FullName;
            user.AvatarUrl = dto.AvatarUrl;
            user.Language = dto.Language;
            user.NotificationsEnabled = dto.NotificationsEnabled;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [HttpPut("password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("privacy")]
        public IActionResult GetPrivacyPolicy()
        {
            return Ok(new { content = "Here is your privacy policy text..." });
        }

        [AllowAnonymous]
        [HttpGet("support")]
        public IActionResult GetSupportCenter()
        {
            return Ok(new { email = "support@example.com", phone = "+201234567890" });
        }
    }
}