using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiOdonto.Api.Models.Dto;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly ChatService _chatService;

    public ChatController(ChatService chatService)
    {
        _chatService = chatService;
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpPost("sessions")]
    public async Task<IActionResult> CreateSession([FromBody] CreateSessionRequest request)
    {
        var session = await _chatService.CreateSessionAsync(GetUserId(), request.Title);
        return Ok(new { session.Id, session.Title, session.CreatedAt });
    }

    [HttpGet("sessions")]
    public async Task<IActionResult> GetSessions()
    {
        var sessions = await _chatService.GetUserSessionsAsync(GetUserId());
        return Ok(sessions.Select(s => new ChatSessionDto
        {
            Id = s.Id,
            Title = s.Title,
            CreatedAt = s.CreatedAt,
            MessageCount = s.Messages.Count
        }));
    }

    [HttpGet("sessions/{id}")]
    public async Task<IActionResult> GetSession(int id)
    {
        var session = await _chatService.GetSessionWithMessagesAsync(id, GetUserId());
        if (session == null) return NotFound();
        return Ok(session);
    }

    [HttpPost("sessions/{id}/messages")]
    public async Task<IActionResult> SendMessage(int id, [FromBody] SendMessageRequest request)
    {
        var message = await _chatService.SendMessageAsync(id, GetUserId(), request.Message);
        return Ok(message);
    }

    [HttpDelete("sessions/{id}")]
    public async Task<IActionResult> DeleteSession(int id)
    {
        var deleted = await _chatService.DeleteSessionAsync(id, GetUserId());
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class CreateSessionRequest
{
    public string Title { get; set; } = string.Empty;
}
