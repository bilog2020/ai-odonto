using Microsoft.EntityFrameworkCore;
using Moq;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class ChatServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateSession_ReturnsSessionWithActivePrompt()
    {
        using var context = CreateContext();

        var prompt = new PromptVersion
        {
            Version = "1.0.0",
            SystemPrompt = "You are a dental assistant.",
            Author = "admin",
            Description = "Initial prompt",
            Status = PromptStatus.Production
        };
        context.PromptVersions.Add(prompt);
        await context.SaveChangesAsync();

        var vectorSearchMock = new Mock<IVectorSearchService>();
        var llmServiceMock = new Mock<ILlmService>();
        var service = new ChatService(context, vectorSearchMock.Object, llmServiceMock.Object);

        var session = await service.CreateSessionAsync("user-1", "Mi primera consulta");

        Assert.True(session.Id > 0);
        Assert.Equal(prompt.Id, session.PromptVersionId);
        Assert.Equal("Mi primera consulta", session.Title);
    }

    [Fact]
    public async Task SendMessage_ReturnsAgentResponseWithSources()
    {
        using var context = CreateContext();

        var prompt = new PromptVersion
        {
            Version = "1.0.0",
            SystemPrompt = "You are a dental assistant. Always cite sources.",
            Author = "admin",
            Description = "Initial prompt",
            Status = PromptStatus.Production
        };
        context.PromptVersions.Add(prompt);

        var doc = new Document
        {
            Title = "Guía de Caries",
            SourceType = "PDF",
            OriginalFileName = "caries.pdf",
            Version = "1.0",
            Status = DocumentStatus.Active,
            EffectiveDate = DateTime.UtcNow
        };
        context.Documents.Add(doc);
        await context.SaveChangesAsync();

        var chunk = new DocumentChunk
        {
            DocumentId = doc.Id,
            ChunkIndex = 0,
            Content = "La caries dental es una enfermedad multifactorial...",
            Embedding = new float[1536],
            TokenCount = 50
        };
        context.DocumentChunks.Add(chunk);
        await context.SaveChangesAsync();

        var session = new ChatSession
        {
            UserId = "user-1",
            Title = "Consulta caries",
            PromptVersionId = prompt.Id
        };
        context.ChatSessions.Add(session);
        await context.SaveChangesAsync();

        var vectorSearchMock = new Mock<IVectorSearchService>();
        vectorSearchMock
            .Setup(x => x.SearchAsync(It.IsAny<string>(), It.IsAny<int>()))
            .ReturnsAsync(new List<DocumentChunk> { chunk });

        var llmServiceMock = new Mock<ILlmService>();
        llmServiceMock
            .Setup(x => x.GenerateResponseAsync(
                It.IsAny<string>(), It.IsAny<List<ChatMessage>>(),
                It.IsAny<List<DocumentChunk>>(), It.IsAny<string>()))
            .ReturnsAsync("La caries dental es una enfermedad multifactorial [Guía de Caries, p.1]");

        var service = new ChatService(context, vectorSearchMock.Object, llmServiceMock.Object);

        var response = await service.SendMessageAsync(session.Id, "user-1", "¿Qué es la caries?");

        Assert.Contains("caries", response.Content, StringComparison.OrdinalIgnoreCase);
        Assert.NotEmpty(response.SourceChunkIds);

        var messages = await context.ChatMessages
            .Where(m => m.SessionId == session.Id)
            .ToListAsync();
        Assert.Equal(2, messages.Count);
    }
}
