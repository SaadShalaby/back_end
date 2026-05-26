using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Hubs;
using MedicalApp.API.Models;
using MedicalApp.API.Models.Enums;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Services.Implementation
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;
        private readonly IHubContext<ChatHub> _chatHubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        public MessageService(
            AppDbContext context, 
            IFileStorageService fileStorage,
            IHubContext<ChatHub> chatHubContext,
            IHubContext<NotificationHub> notificationHubContext)
        {
            _context = context;
            _fileStorage = fileStorage;
            _chatHubContext = chatHubContext;
            _notificationHubContext = notificationHubContext;
        }

        // ─────────────────────────────────────────────
        // Get Conversations (Paginated)
        // ─────────────────────────────────────────────
        public async Task<PagedResultDto<ConversationDto>> GetConversationsAsync(
            string userId, int page, int pageSize)
        {
            // ✅ Step 1: Get (ConversationId, OtherId) directly from DB — no string parsing
            var participantPairs = await _context.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Select(m => new
                {
                    m.ConversationId,
                    OtherId = m.SenderId == userId ? m.ReceiverId : m.SenderId
                })
                .Distinct()
                .ToListAsync();

            // Map conversationId -> otherId
            var conversationOtherMap = participantPairs
                .GroupBy(x => x.ConversationId)
                .ToDictionary(g => g.Key, g => g.First().OtherId, StringComparer.OrdinalIgnoreCase);

            // ✅ Step 2: Get conversation stats
            var rawConversations = await _context.Messages
                .AsNoTracking()
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .GroupBy(m => m.ConversationId)
                .Select(g => new
                {
                    ConversationId = g.Key,
                    LastMessage = g.OrderByDescending(x => x.SentAt).Select(x => x.Content).FirstOrDefault(),
                    LastMessageTime = g.Max(x => x.SentAt),
                    UnreadCount = g.Count(x => x.ReceiverId == userId && !x.IsRead)
                })
                .OrderByDescending(c => c.LastMessageTime)
                .ToListAsync();

            var totalCount = rawConversations.Count;
            var paged = rawConversations.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // ✅ Step 3: Collect other user IDs as plain List (EF Core translates correctly)
            var otherUserIds = paged
                .Select(c => conversationOtherMap.TryGetValue(c.ConversationId, out var id) ? id : null)
                .Where(id => id != null)
                .Distinct()
                .ToList();

            // ✅ Step 4: Batch-load user info
            var usersRaw = await _context.Users
                .Where(u => otherUserIds.Contains(u.Id))
                .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
                .ToListAsync();

            var usersDict = usersRaw.ToDictionary(u => u.Id, StringComparer.OrdinalIgnoreCase);

            // ✅ Step 5: Load favorites
            var favoriteIds = (await _context.FavoriteChats
                .Where(f => f.UserId == userId)
                .Select(f => f.ConversationId)
                .ToListAsync())
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // ✅ Step 6: Build DTOs
            var dtos = paged.Select(c =>
            {
                conversationOtherMap.TryGetValue(c.ConversationId, out var otherId);
                usersDict.TryGetValue(otherId ?? "", out var info);

                return new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    UserId = otherId ?? "",
                    UserName = info?.FullName ?? "Unknown User",
                    UserImage = info?.AvatarUrl,
                    LastMessage = c.LastMessage,
                    LastMessageTime = c.LastMessageTime,
                    UnreadCount = c.UnreadCount,
                    IsFavorite = favoriteIds.Contains(c.ConversationId)
                };
            }).ToList();

            return new PagedResultDto<ConversationDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─────────────────────────────────────────────
        // Get Messages (Paginated — oldest last loaded first)
        // ─────────────────────────────────────────────
        public async Task<PagedResultDto<MessageDto>> GetMessagesAsync(
            string conversationId, string userId, int page, int pageSize)
        {
            // Security: ensure user is participant
            var isParticipant = await _context.Messages
                .AnyAsync(m => m.ConversationId == conversationId &&
                               (m.SenderId == userId || m.ReceiverId == userId));

            if (!isParticipant)
                throw new UnauthorizedAccessException("Access denied to this conversation.");

            var totalCount = await _context.Messages
                .CountAsync(m => m.ConversationId == conversationId);

            var messages = await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderByDescending(m => m.SentAt)           // newest first for reverse scroll
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new MessageDto
                {
                    Id = m.Id,
                    SenderId = m.SenderId,
                    ReceiverId = m.ReceiverId,
                    Message = m.Content,
                    MessageType = m.MessageType,
                    AttachmentUrl = m.AttachmentUrl,
                    AttachmentName = m.AttachmentName,
                    IsPinned = m.IsPinned,
                    IsRead = m.IsRead,
                    ReadAt = m.ReadAt,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            messages.Reverse(); // restore chronological order

            return new PagedResultDto<MessageDto>
            {
                Items = messages,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─────────────────────────────────────────────
        // Send Text Message (JSON)
        // ─────────────────────────────────────────────
        public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new ArgumentException("Message content cannot be empty.");

            var conversationId = GenerateConversationId(senderId, dto.ReceiverId);

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                ConversationId = conversationId,
                Content = dto.Message,
                MessageType = MessageType.Text
            };

            _context.Messages.Add(message);
            _context.Notifications.Add(new Notification
            {
                UserId = dto.ReceiverId,
                Title = "New Message",
                Body = dto.Message,
                Type = "Message"
            });

            await _context.SaveChangesAsync();
            
            // Real-time unread count update for the receiver
            var unreadCount = await GetUnreadCountAsync(dto.ReceiverId);
            await _notificationHubContext.Clients.Group($"user_{dto.ReceiverId}")
                .SendAsync("UnreadCountUpdated", unreadCount);

            return MapToDto(message);
        }

        // ─────────────────────────────────────────────
        // Send File Message (multipart/form-data)
        // ─────────────────────────────────────────────
        public async Task<MessageDto> SendFileMessageAsync(string senderId, SendFileMessageDto dto)
        {
            if (dto.Attachment == null || dto.Attachment.Length == 0)
                throw new ArgumentException("Attachment is required.");

            var conversationId = GenerateConversationId(senderId, dto.ReceiverId);

            var folder = dto.MessageType switch
            {
                MessageType.Voice => "voice",
                MessageType.Image => "images",
                _ => "files"
            };

            var attachmentUrl = await _fileStorage.SaveFileAsync(dto.Attachment, folder);

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = dto.ReceiverId,
                ConversationId = conversationId,
                Content = dto.Caption,
                MessageType = dto.MessageType,
                AttachmentUrl = attachmentUrl,
                AttachmentName = dto.Attachment.FileName
            };

            _context.Messages.Add(message);
            _context.Notifications.Add(new Notification
            {
                UserId = dto.ReceiverId,
                Title = "New Message",
                Body = dto.Caption ?? $"Sent a {dto.MessageType.ToString().ToLower()}",
                Type = "Message"
            });

            await _context.SaveChangesAsync();

            // Real-time unread count update for the receiver
            var unreadCount = await GetUnreadCountAsync(dto.ReceiverId);
            await _notificationHubContext.Clients.Group($"user_{dto.ReceiverId}")
                .SendAsync("UnreadCountUpdated", unreadCount);

            return MapToDto(message);
        }

        // ─────────────────────────────────────────────
        // Mark as Read
        // ─────────────────────────────────────────────
        public async Task MarkAsReadAsync(string conversationId, string userId)
        {
            var unread = await _context.Messages
                .Where(m => m.ConversationId == conversationId &&
                            m.ReceiverId == userId &&
                            !m.IsRead)
                .ToListAsync();

            if (!unread.Any()) return;

            foreach (var m in unread)
            {
                m.IsRead = true;
                m.ReadAt = DateTime.Now;
            }
            await _context.SaveChangesAsync();

            // 1. Notify the sender immediately to show double checks
            await _chatHubContext.Clients.Group(conversationId)
                .SendAsync("MessagesRead", new { ConversationId = conversationId, ReadBy = userId });

            // 2. Update receiver's own unread count badge
            var newCount = await GetUnreadCountAsync(userId);
            await _notificationHubContext.Clients.Group($"user_{userId}")
                .SendAsync("UnreadCountUpdated", newCount);
        }

        // ─────────────────────────────────────────────
        // Unread Count
        // ─────────────────────────────────────────────
        public async Task<int> GetUnreadCountAsync(string userId)
            => await _context.Messages.AsNoTracking().CountAsync(m => m.ReceiverId == userId && !m.IsRead);

        // ─────────────────────────────────────────────
        // Pin / Unpin Message
        // ─────────────────────────────────────────────
        public async Task<MessageDto> PinMessageAsync(int messageId, string userId)
            => await SetPinAsync(messageId, userId, true);

        public async Task<MessageDto> UnpinMessageAsync(int messageId, string userId)
            => await SetPinAsync(messageId, userId, false);

        private async Task<MessageDto> SetPinAsync(int messageId, string userId, bool pin)
        {
            var message = await _context.Messages.FindAsync(messageId)
                ?? throw new KeyNotFoundException("Message not found.");

            if (message.SenderId != userId && message.ReceiverId != userId)
                throw new UnauthorizedAccessException("Access denied.");

            message.IsPinned = pin;
            await _context.SaveChangesAsync();

            return MapToDto(message);
        }

        // ─────────────────────────────────────────────
        // Get Pinned Messages
        // ─────────────────────────────────────────────
        public async Task<List<MessageDto>> GetPinnedMessagesAsync(string conversationId, string userId)
        {
            var isParticipant = await _context.Messages
                .AnyAsync(m => m.ConversationId == conversationId &&
                               (m.SenderId == userId || m.ReceiverId == userId));

            if (!isParticipant)
                throw new UnauthorizedAccessException("Access denied.");

            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId && m.IsPinned)
                .OrderBy(m => m.SentAt)
                .Select(m => MapToDto(m))
                .ToListAsync();
        }

        // ─────────────────────────────────────────────
        // Favorite Chats
        // ─────────────────────────────────────────────
        public async Task<bool> ToggleFavoriteAsync(string conversationId, string userId)
        {
            var existing = await _context.FavoriteChats
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ConversationId == conversationId);

            if (existing != null)
            {
                _context.FavoriteChats.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // removed
            }

            _context.FavoriteChats.Add(new FavoriteChat
            {
                UserId = userId,
                ConversationId = conversationId
            });
            await _context.SaveChangesAsync();
            return true; // added
        }

        public async Task<List<ConversationDto>> GetFavoritesAsync(string userId)
        {
            var favorites = await _context.FavoriteChats
                .Where(f => f.UserId == userId)
                .Select(f => f.ConversationId)
                .ToListAsync();

            if (!favorites.Any())
                return new List<ConversationDto>();

            var conversations = await _context.Messages
                .AsNoTracking()
                .Where(m => favorites.Contains(m.ConversationId) &&
                            (m.SenderId == userId || m.ReceiverId == userId))
                .GroupBy(m => m.ConversationId)
                .Select(g => new
                {
                    ConversationId = g.Key,
                    LastMessage = g.OrderByDescending(x => x.SentAt).Select(x => x.Content).FirstOrDefault(),
                    LastMessageTime = g.Max(x => x.SentAt),
                    UnreadCount = g.Count(x => x.ReceiverId == userId && !x.IsRead)
                })
                .ToListAsync();

            var otherIds = conversations
                .Select(c => c.ConversationId.Split('_'))
                .Where(p => p.Length == 2)
                .Select(p => string.Equals(p[0], userId, StringComparison.OrdinalIgnoreCase) ? p[1] : p[0])
                .Distinct()
                .ToList();

            var usersInfo = await _context.Users
                .Where(u => otherIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, StringComparer.OrdinalIgnoreCase);

            return conversations.Select(c =>
            {
                var parts = c.ConversationId.Split('_');
                var otherId = parts.Length == 2
                    ? (string.Equals(parts[0], userId, StringComparison.OrdinalIgnoreCase) ? parts[1] : parts[0])
                    : null;

                usersInfo.TryGetValue(otherId ?? "", out var info);

                return new ConversationDto
                {
                    ConversationId = c.ConversationId,
                    UserId = otherId ?? "",
                    UserName = info?.FullName ?? "Unknown User",
                    UserImage = info?.AvatarUrl,
                    LastMessage = c.LastMessage,
                    LastMessageTime = c.LastMessageTime,
                    UnreadCount = c.UnreadCount,
                    IsFavorite = true
                };
            }).ToList();
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────
        private static string GenerateConversationId(string u1, string u2)
            => string.Compare(u1, u2, StringComparison.OrdinalIgnoreCase) < 0
                ? $"{u1}_{u2}"
                : $"{u2}_{u1}";

        private static MessageDto MapToDto(Message m) => new()
        {
            Id = m.Id,
            SenderId = m.SenderId,
            ReceiverId = m.ReceiverId,
            Message = m.Content,
            MessageType = m.MessageType,
            AttachmentUrl = m.AttachmentUrl,
            AttachmentName = m.AttachmentName,
            IsPinned = m.IsPinned,
            IsRead = m.IsRead,
            ReadAt = m.ReadAt,
            SentAt = m.SentAt
        };
    }
}
