using System.Text;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class ClaudeLlmService : ILlmService
{
    private readonly Kernel _kernel;

    public ClaudeLlmService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<string> GenerateResponseAsync(
        string systemPrompt,
        List<ChatMessage> conversationHistory,
        List<DocumentChunk> relevantChunks,
        string userMessage)
    {
        var chatService = _kernel.GetRequiredService<IChatCompletionService>();

        var contextPrompt = BuildContextPrompt(relevantChunks);
        var fullSystemPrompt = $"{systemPrompt}\n\n{contextPrompt}";

        var chatHistory = new ChatHistory(fullSystemPrompt);

        foreach (var msg in conversationHistory)
        {
            if (msg.Role == "user")
                chatHistory.AddUserMessage(msg.Content);
            else if (msg.Role == "assistant")
                chatHistory.AddAssistantMessage(msg.Content);
        }

        chatHistory.AddUserMessage(userMessage);

        var response = await chatService.GetChatMessageContentAsync(chatHistory);

        return response.Content ?? "No se pudo generar una respuesta.";
    }

    public static string BuildContextPrompt(List<DocumentChunk> chunks)
    {
        if (chunks.Count == 0)
        {
            return "No se encontraron documentos relevantes en la base de conocimiento. " +
                   "Indica al usuario que tu respuesta no está respaldada por fuentes oficiales.";
        }

        var sb = new StringBuilder();
        sb.AppendLine("## Documentos de referencia");
        sb.AppendLine("Usa SOLO la siguiente información para responder. Cita la fuente entre corchetes.");
        sb.AppendLine();

        foreach (var chunk in chunks)
        {
            var docTitle = chunk.Document?.Title ?? "Documento desconocido";
            sb.AppendLine($"### [{docTitle}]");
            sb.AppendLine(chunk.Content);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
