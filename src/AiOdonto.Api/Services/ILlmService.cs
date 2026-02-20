using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public interface ILlmService
{
    Task<string> GenerateResponseAsync(
        string systemPrompt,
        List<ChatMessage> conversationHistory,
        List<DocumentChunk> relevantChunks,
        string userMessage);
}
