namespace MedicalApp.API.Services.Interfaces
{
    public interface INotificationService
    {
        /// <summary>
        /// يحفظ الإشعار في قاعدة البيانات ويبعته real-time عبر SignalR
        /// </summary>
        Task SendNotificationAsync(string targetUserId, string title, string body, string type);
    }
}
