namespace AiOdonto.Api.Models;

public class PromptVersion
{
    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PromptStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ActivatedAt { get; set; }
}
