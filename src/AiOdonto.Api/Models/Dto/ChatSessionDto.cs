namespace AiOdonto.Api.Models.Dto;

public class ChatSessionDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int MessageCount { get; set; }
}
