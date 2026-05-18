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
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;

        public ProfileController(UserManager<ApplicationUser> userManager, AppDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound("User not found");

            // 1. Calculate Completed Sessions
            var sessionsCompleted = await _context.DoctorSessions
                .CountAsync(s => s.PatientId == userId && s.Status == "Completed");

            // 2. Calculate Completed Exercises
            var exercisesCompleted = await _context.AssessmentResults
                .CountAsync(a => a.UserId == userId);

            // 3. Calculate Active Days
            var activeDays = 1;
            var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
            if (patient != null)
            {
                activeDays = Math.Max(1, (int)(DateTime.Now - patient.RegisteredAt).TotalDays);
            }

            var profile = new ProfileDto
            {
                FullName = user.FullName ?? "User Name",
                Email = user.Email ?? "",
                AvatarUrl = user.AvatarUrl ?? "/images/default-user.png",
                SessionsCompleted = sessionsCompleted,
                ExercisesCompleted = exercisesCompleted,
                ActiveDays = activeDays
            };

            return Ok(profile);
        }

        [HttpPost("update-info")]
        public async Task<IActionResult> UpdateInfo([FromBody] UpdateInfoDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(dto.FullName))
            {
                user.FullName = dto.FullName;

                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == userId);
                if (doctor != null)
                {
                    doctor.Name = dto.FullName;
                }
            }

            if (dto.Age.HasValue || !string.IsNullOrEmpty(dto.Gender))
            {
                var patient = await _context.Patients.FirstOrDefaultAsync(p => p.UserId == userId);
                if (patient != null)
                {
                    if (dto.Age.HasValue) patient.Age = dto.Age.Value;
                    if (!string.IsNullOrEmpty(dto.Gender)) patient.Gender = dto.Gender;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Profile updated successfully", newName = user.FullName });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please select a file.");

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/profiles");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            user.AvatarUrl = "/uploads/profiles/" + fileName;
            await _userManager.UpdateAsync(user);

            return Ok(new
            {
                message = "Profile picture uploaded successfully!",
                avatarUrl = user.AvatarUrl
            });
        }

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId!);

            if (user == null) return NotFound();

            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (result.Succeeded)
            {
                return Ok(new { message = "Password changed successfully!" });
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("saved-resources")]
        public async Task<IActionResult> GetSavedResources()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var savedItems = await _context.SavedItems
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.SavedAt)
                .ToListAsync();

            var responseList = new List<SavedItemResponseDto>();

            foreach (var item in savedItems)
            {
                object? data = null;

                if (item.ContentType == "post")
                {
                    var p = await _context.Posts.Include(x => x.User).FirstOrDefaultAsync(x => x.Id == item.ItemId);
                    if (p != null)
                    {
                        data = new PostResponseDto
                        {
                            Id = p.Id,
                            Content = p.Content,
                            ImageUrl = p.ImageUrl,
                            UserName = p.User.FullName,
                            UserAvatar = p.User.AvatarUrl ?? "/images/default-user.png",
                            CreatedAt = p.CreatedAt,
                            UpdatedAt = p.UpdatedAt,
                            IsEdited = p.UpdatedAt.HasValue,
                            IsOwner = p.UserId == userId,
                            IsSaved = true,
                            LikesCount = await _context.Likes.CountAsync(l => l.PostId == p.Id),
                            CommentsCount = await _context.Comments.CountAsync(c => c.PostId == p.Id)
                        };
                    }
                }
                else
                {
                    var r = await _context.Resources.FirstOrDefaultAsync(x => x.Id == item.ItemId);
                    if (r != null)
                    {
                        data = new ResourceDto
                        {
                            Id = r.Id,
                            Title = r.Title,
                            Description = r.Description,
                            CoverImageUrl = r.CoverImageUrl,
                            Type = r.Type.ToString(),
                            Url = r.Url,
                            Duration = r.Duration,
                            FileSize = r.FileSize,
                            CreatedDate = r.CreatedDate,
                            IsSaved = true
                        };
                    }
                }

                if (data != null)
                {
                    responseList.Add(new SavedItemResponseDto
                    {
                        SaveId = item.Id,
                        ContentType = item.ContentType,
                        SavedAt = item.SavedAt,
                        Data = data
                    });
                }
            }

            return Ok(responseList);
        }
    }
}
