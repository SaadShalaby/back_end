using MedicalApp.API.Data;
using MedicalApp.API.DTOs;
using MedicalApp.API.Models;
using MedicalApp.API.Models.Enums;
using MedicalApp.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedicalApp.API.Services.Implementation
{
    public class AiChatService : IAiChatService
    {
        private readonly AppDbContext _context;
        private readonly IFileStorageService _fileStorage;

        public AiChatService(AppDbContext context, IFileStorageService fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        // ─────────────────────────────────────────────
        // Create Session
        // ─────────────────────────────────────────────
        public async Task<ChatSessionDto> CreateSessionAsync(string userId, string? title = null)
        {
            var session = new ChatSession
            {
                UserId = userId,
                Title = string.IsNullOrWhiteSpace(title) ? "New Chat" : title.Trim(),
                IsTitleGenerated = false,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();

            return MapSessionToDto(session, null, 0);
        }

        // ─────────────────────────────────────────────
        // Get All Sessions (Paginated)
        // ─────────────────────────────────────────────
        public async Task<PagedResultDto<ChatSessionDto>> GetSessionsAsync(
            string userId, int page, int pageSize)
        {
            var total = await _context.ChatSessions
                .CountAsync(s => s.UserId == userId);

            var sessions = await _context.ChatSessions
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    Session = s,
                    LastMessage = s.Messages
                        .OrderByDescending(m => m.SentAt)
                        .Select(m => m.Message)
                        .FirstOrDefault(),
                    MessageCount = s.Messages.Count
                })
                .ToListAsync();

            var dtos = sessions.Select(x =>
                MapSessionToDto(x.Session, x.LastMessage, x.MessageCount)).ToList();

            return new PagedResultDto<ChatSessionDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─────────────────────────────────────────────
        // Get Messages of a Session (Paginated)
        // ─────────────────────────────────────────────
        public async Task<PagedResultDto<BotMessageResponseDto>> GetSessionMessagesAsync(
            int sessionId, string userId, int page, int pageSize)
        {
            await EnsureSessionOwnerAsync(sessionId, userId);

            var total = await _context.BotMessages
                .CountAsync(m => m.SessionId == sessionId);

            var messages = await _context.BotMessages
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.SentAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new BotMessageResponseDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Message = m.Message,
                    Emotion = m.Emotion,
                    MessageType = m.MessageType,
                    AttachmentUrl = m.AttachmentUrl,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            messages.Reverse(); // chronological order

            return new PagedResultDto<BotMessageResponseDto>
            {
                Items = messages,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        // ─────────────────────────────────────────────
        // Save a Message to a Session
        // ─────────────────────────────────────────────
        public async Task<BotMessageResponseDto> SaveMessageAsync(
            string userId, int sessionId, SendBotMessageDto dto)
        {
            if (dto.Sender != "Patient" && dto.Sender != "Depo")
                throw new ArgumentException("Sender must be 'Patient' or 'Depo'.");

            var session = await EnsureSessionOwnerAsync(sessionId, userId);

            string? attachmentUrl = null;
            if (dto.Attachment != null)
                attachmentUrl = await _fileStorage.SaveFileAsync(dto.Attachment, "chat-attachments");

            var botMessage = new BotMessage
            {
                PatientId = userId,
                SessionId = sessionId,
                Sender = dto.Sender,
                Message = dto.Message,
                Emotion = dto.Emotion,
                MessageType = dto.MessageType,
                AttachmentUrl = attachmentUrl,
                SentAt = DateTime.Now
            };

            _context.BotMessages.Add(botMessage);

            // Update session timestamp
            session.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return new BotMessageResponseDto
            {
                Id = botMessage.Id,
                Sender = botMessage.Sender,
                Message = botMessage.Message,
                Emotion = botMessage.Emotion,
                MessageType = botMessage.MessageType,
                AttachmentUrl = botMessage.AttachmentUrl,
                SentAt = botMessage.SentAt
            };
        }

        // ─────────────────────────────────────────────
        // Auto-generate session title from first message
        // ─────────────────────────────────────────────
        public async Task<string> GenerateTitleAsync(int sessionId, string userId)
        {
            var session = await EnsureSessionOwnerAsync(sessionId, userId);

            // Grab the first user message to derive a title
            var firstMsg = await _context.BotMessages
                .Where(m => m.SessionId == sessionId && m.Sender == "Patient")
                .OrderBy(m => m.SentAt)
                .Select(m => m.Message)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(firstMsg))
                return session.Title;

            // Generate a concise title (max 60 chars, trimmed to last whole word)
            var generated = firstMsg.Length <= 60
                ? firstMsg
                : firstMsg[..60].TrimEnd().Split(' ').SkipLast(1).DefaultIfEmpty("New Chat")
                    .Aggregate((a, b) => $"{a} {b}") + "…";

            session.Title = generated;
            session.IsTitleGenerated = true;
            session.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return session.Title;
        }

        // ─────────────────────────────────────────────
        // Manual title update
        // ─────────────────────────────────────────────
        public async Task UpdateSessionTitleAsync(int sessionId, string userId, string title)
        {
            var session = await EnsureSessionOwnerAsync(sessionId, userId);
            session.Title = title.Trim();
            session.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────
        // Delete Session
        // ─────────────────────────────────────────────
        public async Task DeleteSessionAsync(int sessionId, string userId)
        {
            var session = await EnsureSessionOwnerAsync(sessionId, userId);
            _context.ChatSessions.Remove(session);
            await _context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────
        // Get Context (last N messages for AI memory)
        // ─────────────────────────────────────────────
        public async Task<List<BotMessageResponseDto>> GetContextAsync(
            int sessionId, string userId, int maxMessages = 20)
        {
            await EnsureSessionOwnerAsync(sessionId, userId);

            var messages = await _context.BotMessages
                .Where(m => m.SessionId == sessionId)
                .OrderByDescending(m => m.SentAt)
                .Take(maxMessages)
                .Select(m => new BotMessageResponseDto
                {
                    Id = m.Id,
                    Sender = m.Sender,
                    Message = m.Message,
                    Emotion = m.Emotion,
                    MessageType = m.MessageType,
                    AttachmentUrl = m.AttachmentUrl,
                    SentAt = m.SentAt
                })
                .ToListAsync();

            messages.Reverse(); // chronological
            return messages;
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────
        private async Task<ChatSession> EnsureSessionOwnerAsync(int sessionId, string userId)
        {
            var session = await _context.ChatSessions
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
                ?? throw new UnauthorizedAccessException("Session not found or access denied.");

            return session;
        }

        private static ChatSessionDto MapSessionToDto(
            ChatSession session, string? lastMessage, int messageCount) => new()
        {
            Id = session.Id,
            Title = session.Title,
            IsTitleGenerated = session.IsTitleGenerated,
            CreatedAt = session.CreatedAt,
            UpdatedAt = session.UpdatedAt,
            LastMessage = lastMessage,
            MessageCount = messageCount
        };
    }
}
