using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using MedicalApp.API.Services;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtService _jwtService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly IFileStorageService _fileStorageService;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<ApplicationUser> userManager, IFileStorageService fileStorageService, JwtService jwtService, AppDbContext context, IConfiguration config, IEmailService emailService)
        {
            _userManager = userManager;
            _jwtService = jwtService;
            _context = context;
            _config = config;
            _fileStorageService = fileStorageService;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, dto.Role);

            if (dto.Role.Equals("Doctor", StringComparison.OrdinalIgnoreCase))
            {
                var doctor = new Doctor
                {
                    UserId = user.Id,
                    Name = dto.FullName,
                    NationalNumber = dto.NationalNumber ?? "",
                    Specialization = dto.Specialization ?? "General",
                    ExperienceYears = dto.ExperienceYears ?? 0,
                    Bio = dto.Bio ?? "",
                };

                await _context.Doctors.AddAsync(doctor);
            }
            else
            {
                _context.Patients.Add(new Patient
                {
                    UserId = user.Id,
                   
                });
            }

            await _context.SaveChangesAsync();
            var token = _jwtService.CreateToken(user, dto.Role);

            return Ok(new { token, role = dto.Role, profileImage = user.AvatarUrl });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid Email or Password");

            var roles = await _userManager.GetRolesAsync(user);

            // التوكن بيتم إنشاؤه بناءً على الـ Role
            var token = _jwtService.CreateToken(user, roles.First());

            return Ok(new
            {
                token,
                role = roles.First(),
                profileImage = user.AvatarUrl,
                rememberMe = dto.RememberMe
            });
        }

        // --- 🔵 ميثود نسيت كلمة السر (تم تعديلها للوضع التجريبي) ---
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Email not found");

            // توليد كود عشوائي مكون من 6 أرقام
            var resetCode = new Random().Next(100000, 999999).ToString();
            user.ResetToken = resetCode;
            user.ResetTokenExpires = DateTime.Now.AddHours(1);
            await _userManager.UpdateAsync(user);

            // إرسال الكود باستخدام EmailService
            await _emailService.SendOtpEmailAsync(user.Email!, resetCode);

            return Ok(new
            {
                message = "Reset code generated successfully.",
                debugCode = resetCode // الكود ده هيظهرلك في الـ Response عشان الموبايل يجربه
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.ResetToken != dto.Code || user.ResetTokenExpires < DateTime.Now)
                return BadRequest("Invalid or expired code.");

            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded) return BadRequest(removeResult.Errors);

            var addResult = await _userManager.AddPasswordAsync(user, dto.NewPassword);
            if (!addResult.Succeeded) return BadRequest(addResult.Errors);

            user.ResetToken = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "Password changed successfully!" });
        }


    }
}