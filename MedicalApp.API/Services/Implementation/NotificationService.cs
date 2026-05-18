using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Hubs;
using MedicalApp.API.Models;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace MedicalApp.API.Services.Implementation
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationService(AppDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        /// <summary>
        /// يحفظ الإشعار في DB ويبعته real-time لليوزر المستهدف
        /// </summary>
        public async Task SendNotificationAsync(string targetUserId, string title, string body, string type)
        {
            // 1️⃣ حفظ الإشعار في قاعدة البيانات
            var notification = new Notification
            {
                UserId = targetUserId,
                Title = title,
                Body = body,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 2️⃣ إرسال الإشعار real-time عبر SignalR للـ Group بتاعت اليوزر
            var notificationDto = new NotificationDto
            {
                Id = notification.Id,
                Title = notification.Title,
                Body = notification.Body,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _hubContext.Clients
                .Group($"user_{targetUserId}")
                .SendAsync("ReceiveNotification", notificationDto);
        }
    }
}
