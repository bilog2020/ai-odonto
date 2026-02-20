using System.ComponentModel.DataAnnotations;

namespace AiOdonto.Api.Models.Dto;

public class SendMessageRequest
{
    [Required]
    public string Message { get; set; } = string.Empty;
}
