using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class ChatService
{
    private readonly AppDbContext _context;
    private readonly IVectorSearchService _vectorSearch;
    private readonly ILlmService _llmService;

    public ChatService(
        AppDbContext context,
        IVectorSearchService vectorSearch,
        ILlmService llmService)
    {
        _context = context;
        _vectorSearch = vectorSearch;
        _llmService = llmService;
    }

    public async Task<ChatSession> CreateSessionAsync(string userId, string title)
    {
        var activePrompt = await _context.PromptVersions
            .Where(p => p.Status == PromptStatus.Production)
            .OrderByDescending(p => p.ActivatedAt ?? p.CreatedAt)
            .FirstOrDefaultAsync()
            ?? throw new InvalidOperationException("No active prompt version found");

        var session = new ChatSession
        {
            UserId = userId,
            Title = title,
            PromptVersionId = activePrompt.Id
        };

        _context.ChatSessions.Add(session);
        await _context.SaveChangesAsync();
        return session;
    }

    public async Task<List<ChatSession>> GetUserSessionsAsync(string userId)
    {
        return await _context.ChatSessions
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();
    }

    public async Task<ChatSession?> GetSessionWithMessagesAsync(int sessionId, string userId)
    {
        return await _context.ChatSessions
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
    }

    public async Task<ChatMessage> SendMessageAsync(int sessionId, string userId, string userMessage)
    {
        var session = await _context.ChatSessions
            .Include(s => s.PromptVersion)
            .Include(s => s.Messages.OrderBy(m => m.CreatedAt))
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId)
            ?? throw new InvalidOperationException("Session not found");

        // Save user message
        var userMsg = new ChatMessage
        {
            SessionId = sessionId,
            Role = "user",
            Content = userMessage
        };
        _context.ChatMessages.Add(userMsg);
        await _context.SaveChangesAsync();

        // RAG: retrieve relevant chunks
        var relevantChunks = await _vectorSearch.SearchAsync(userMessage);

        // Generate response
        var responseText = await _llmService.GenerateResponseAsync(
            session.PromptVersion.SystemPrompt,
            session.Messages.ToList(),
            relevantChunks,
            userMessage);

        // Save assistant message
        var assistantMsg = new ChatMessage
        {
            SessionId = sessionId,
            Role = "assistant",
            Content = responseText,
            SourceChunkIds = relevantChunks.Select(c => c.Id).ToList()
        };
        _context.ChatMessages.Add(assistantMsg);
        await _context.SaveChangesAsync();

        return assistantMsg;
    }

    public async Task<bool> DeleteSessionAsync(int sessionId, string userId)
    {
        var session = await _context.ChatSessions
            .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);
        if (session == null) return false;

        _context.ChatSessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }
}
