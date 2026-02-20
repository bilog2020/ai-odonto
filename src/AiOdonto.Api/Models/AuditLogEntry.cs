namespace AiOdonto.Api.Models;

public class AuditLogEntry
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public int? SessionId { get; set; }
    public int? PromptVersionId { get; set; }
    public List<int> RetrievedChunkIds { get; set; } = new();
    public string? RequestPayload { get; set; }
    public string? ResponseSummary { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int DurationMs { get; set; }
}
