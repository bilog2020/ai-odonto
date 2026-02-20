using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class AuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogQueryAsync(
        string userId, int sessionId, int promptVersionId,
        List<int> retrievedChunkIds, string request, string response, int durationMs)
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            Action = "query",
            SessionId = sessionId,
            PromptVersionId = promptVersionId,
            RetrievedChunkIds = retrievedChunkIds,
            RequestPayload = request,
            ResponseSummary = response.Length > 500 ? response[..500] : response,
            DurationMs = durationMs
        };

        _context.AuditLog.Add(entry);
        await _context.SaveChangesAsync();
    }

    public async Task LogActionAsync(string userId, string action)
    {
        var entry = new AuditLogEntry
        {
            UserId = userId,
            Action = action
        };

        _context.AuditLog.Add(entry);
        await _context.SaveChangesAsync();
    }
}
