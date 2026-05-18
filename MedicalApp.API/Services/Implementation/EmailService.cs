using MedicalApp.API.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace MedicalApp.API.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            string subject = "Your Password Reset Code";
            string body = $@"
            <html>
            <body>
                <h2>Password Reset Request</h2>
                <p>You requested to reset your password.</p>
                <p>Your OTP code is: <strong>{otpCode}</strong></p>
                <p>If you did not request this, please ignore this email.</p>
            </body>
            </html>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpSettings = _config.GetSection("SmtpSettings");
            var host = smtpSettings["Host"];
            var port = int.Parse(smtpSettings["Port"] ?? "587");
            var username = smtpSettings["Username"];
            var password = smtpSettings["Password"];
            var fromEmail = smtpSettings["FromEmail"] ?? username;

            // If SMTP is not configured, fallback to console (Debug)
            if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || username == "your-email@gmail.com")
            {
                System.Diagnostics.Debug.WriteLine("=== E-MAIL SIMULATION ===");
                System.Diagnostics.Debug.WriteLine($"To: {toEmail}");
                System.Diagnostics.Debug.WriteLine($"Subject: {subject}");
                System.Diagnostics.Debug.WriteLine($"Body: {body}");
                System.Diagnostics.Debug.WriteLine("=========================");
                return;
            }

            var message = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            message.To.Add(toEmail);

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
        }
    }
}
