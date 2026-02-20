namespace AiOdonto.Api.Models;

public class ChatSession
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PromptVersionId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public PromptVersion PromptVersion { get; set; } = null!;
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
