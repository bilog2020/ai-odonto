namespace AiOdonto.Api.Models;

public class Document
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DocumentStatus Status { get; set; }
    public DateTime EffectiveDate { get; set; }
    public string? UploadedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
}
