using MedicalApp.API.DTOs;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ─────────────────────────────────────────────
        // GET api/messages/conversations?page=1&pageSize=20
        // ─────────────────────────────────────────────
        [HttpGet("conversations")]
        public async Task<IActionResult> GetConversations(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters.");

            var result = await _messageService.GetConversationsAsync(GetUserId(), page, pageSize);
            return Ok(result);
        }

        // ─────────────────────────────────────────────
        // GET api/messages/conversations/{conversationId}?page=1&pageSize=30
        // ─────────────────────────────────────────────
        [HttpGet("conversations/{conversationId}")]
        public async Task<IActionResult> GetMessages(
            string conversationId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 30)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters.");

            try
            {
                var result = await _messageService.GetMessagesAsync(
                    conversationId, GetUserId(), page, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        // POST api/messages/send  — text message (JSON)
        // ─────────────────────────────────────────────
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(dto.Message))
                return BadRequest("Message content cannot be empty.");

            try
            {
                var message = await _messageService.SendMessageAsync(GetUserId(), dto);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        // POST api/messages/send-file  — voice/image/file (multipart/form-data)
        // ─────────────────────────────────────────────
        [HttpPost("send-file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SendFile([FromForm] SendFileMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var message = await _messageService.SendFileMessageAsync(GetUserId(), dto);
                return Ok(message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        // PUT api/messages/mark-as-read/{chatId}
        // ─────────────────────────────────────────────
        [HttpPut("mark-as-read/{chatId}")]
        public async Task<IActionResult> MarkAsRead(string chatId)
        {
            await _messageService.MarkAsReadAsync(chatId, GetUserId());
            var unreadCount = await _messageService.GetUnreadCountAsync(GetUserId());
            return Ok(new { message = "Messages marked as read.", unreadCount });
        }

        // ─────────────────────────────────────────────
        // GET api/messages/unread-count
        // ─────────────────────────────────────────────
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _messageService.GetUnreadCountAsync(GetUserId());
            return Ok(new { unread = count });
        }

        // ─────────────────────────────────────────────
        // POST api/messages/{messageId}/pin
        // ─────────────────────────────────────────────
        [HttpPost("{messageId:int}/pin")]
        public async Task<IActionResult> PinMessage(int messageId)
        {
            try
            {
                var result = await _messageService.PinMessageAsync(messageId, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ─────────────────────────────────────────────
        // POST api/messages/{messageId}/unpin
        // ─────────────────────────────────────────────
        [HttpPost("{messageId:int}/unpin")]
        public async Task<IActionResult> UnpinMessage(int messageId)
        {
            try
            {
                var result = await _messageService.UnpinMessageAsync(messageId, GetUserId());
                return Ok(result);
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ─────────────────────────────────────────────
        // GET api/messages/conversations/{conversationId}/pinned
        // ─────────────────────────────────────────────
        [HttpGet("conversations/{conversationId}/pinned")]
        public async Task<IActionResult> GetPinnedMessages(string conversationId)
        {
            try
            {
                var result = await _messageService.GetPinnedMessagesAsync(
                    conversationId, GetUserId());
                return Ok(result);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        // ─────────────────────────────────────────────
        // POST api/messages/conversations/{conversationId}/favorite
        // ─────────────────────────────────────────────
        [HttpPost("conversations/{conversationId}/favorite")]
        public async Task<IActionResult> ToggleFavorite(string conversationId)
        {
            var isFavorite = await _messageService.ToggleFavoriteAsync(
                conversationId, GetUserId());

            return Ok(new
            {
                isFavorite,
                message = isFavorite ? "Added to favorites." : "Removed from favorites."
            });
        }

        // ─────────────────────────────────────────────
        // GET api/messages/favorites
        // ─────────────────────────────────────────────
        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var result = await _messageService.GetFavoritesAsync(GetUserId());
            return Ok(result);
        }
    }
}