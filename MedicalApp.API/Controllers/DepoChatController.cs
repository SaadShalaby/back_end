using MedicalApp.API.DTOs;
using MedicalApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepoChatController : ControllerBase
    {
        private readonly IAiChatService _aiChatService;

        public DepoChatController(IAiChatService aiChatService)
        {
            _aiChatService = aiChatService;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // ─────────────────────────────────────────────
        // POST api/depochat/sessions
        // Create a new chat session (like starting a new ChatGPT thread)
        // ─────────────────────────────────────────────
        [HttpPost("sessions")]
        public async Task<IActionResult> CreateSession([FromBody] CreateChatSessionDto? dto = null)
        {
            var session = await _aiChatService.CreateSessionAsync(GetUserId(), dto?.Title);
            return Ok(session);
        }

        // ─────────────────────────────────────────────
        // GET api/depochat/sessions?page=1&pageSize=20
        // List all sessions for the current user
        // ─────────────────────────────────────────────
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters.");

            var result = await _aiChatService.GetSessionsAsync(GetUserId(), page, pageSize);
            return Ok(result);
        }

        // ─────────────────────────────────────────────
        // GET api/depochat/sessions/{sessionId}?page=1&pageSize=30
        // Get messages of a specific session (paginated)
        // ─────────────────────────────────────────────
        [HttpGet("sessions/{sessionId:int}")]
        public async Task<IActionResult> GetSessionMessages(
            int sessionId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 30)
        {
            if (page < 1 || pageSize < 1 || pageSize > 100)
                return BadRequest("Invalid pagination parameters.");

            try
            {
                var result = await _aiChatService.GetSessionMessagesAsync(
                    sessionId, GetUserId(), page, pageSize);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return NotFound(ex.Message);
            }
        }

        // ─────────────────────────────────────────────
        // POST api/depochat/sessions/{sessionId}/save
        // Save a message (Patient or Depo) — supports text, voice, image
        // ─────────────────────────────────────────────
        [HttpPost("sessions/{sessionId:int}/save")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveMessage(
            int sessionId,
            [FromForm] SendBotMessageDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _aiChatService.SaveMessageAsync(
                    GetUserId(), sessionId, dto);

                // Auto-generate title after first patient message
                var msgCount = (await _aiChatService.GetSessionMessagesAsync(
                    sessionId, GetUserId(), 1, 1)).TotalCount;

                if (msgCount == 1 && dto.Sender == "Patient")
                    await _aiChatService.GenerateTitleAsync(sessionId, GetUserId());

                return Ok(result);
            }
            catch (UnauthorizedAccessException ex) { return NotFound(ex.Message); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // GET api/depochat/sessions/{sessionId}/context
        // Get last N messages for AI context memory
        // ─────────────────────────────────────────────
        [HttpGet("sessions/{sessionId:int}/context")]
        public async Task<IActionResult> GetContext(
            int sessionId,
            [FromQuery] int maxMessages = 20)
        {
            if (maxMessages < 1 || maxMessages > 100)
                return BadRequest("maxMessages must be between 1 and 100.");

            try
            {
                var context = await _aiChatService.GetContextAsync(
                    sessionId, GetUserId(), maxMessages);
                return Ok(context);
            }
            catch (UnauthorizedAccessException ex) { return NotFound(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // POST api/depochat/sessions/{sessionId}/generate-title
        // Auto-generate title from first user message
        // ─────────────────────────────────────────────
        [HttpPost("sessions/{sessionId:int}/generate-title")]
        public async Task<IActionResult> GenerateTitle(int sessionId)
        {
            try
            {
                var title = await _aiChatService.GenerateTitleAsync(
                    sessionId, GetUserId());
                return Ok(new { title });
            }
            catch (UnauthorizedAccessException ex) { return NotFound(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // PUT api/depochat/sessions/{sessionId}/title
        // Manually rename a session
        // ─────────────────────────────────────────────
        [HttpPut("sessions/{sessionId:int}/title")]
        public async Task<IActionResult> UpdateTitle(
            int sessionId,
            [FromBody] UpdateSessionTitleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _aiChatService.UpdateSessionTitleAsync(
                    sessionId, GetUserId(), dto.Title);
                return Ok(new { message = "Title updated.", title = dto.Title });
            }
            catch (UnauthorizedAccessException ex) { return NotFound(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // DELETE api/depochat/sessions/{sessionId}
        // Delete a session and all its messages (cascade)
        // ─────────────────────────────────────────────
        [HttpDelete("sessions/{sessionId:int}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            try
            {
                await _aiChatService.DeleteSessionAsync(sessionId, GetUserId());
                return Ok(new { message = "Session deleted successfully." });
            }
            catch (UnauthorizedAccessException ex) { return NotFound(ex.Message); }
        }

        // ─────────────────────────────────────────────
        // BACKWARD COMPAT: GET api/depochat/history
        // Returns all messages from the user's first session
        // ─────────────────────────────────────────────
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory()
        {
            var sessions = await _aiChatService.GetSessionsAsync(GetUserId(), 1, 1);
            if (!sessions.Items.Any())
                return Ok(new List<object>());

            var firstSessionId = sessions.Items.First().Id;
            var messages = await _aiChatService.GetContextAsync(
                firstSessionId, GetUserId(), 100);

            return Ok(messages);
        }
    }
}
