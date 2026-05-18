namespace MedicalApp.API.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendOtpEmailAsync(string toEmail, string otpCode);
        Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
