using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Tests.Data;

public class AppDbContextTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CanCreateAndRetrieveDocument()
    {
        using var context = CreateInMemoryContext();

        var doc = new Document
        {
            Title = "Protocolo de Endodoncia",
            SourceType = "PDF",
            OriginalFileName = "proto_endo.pdf",
            Version = "1.0",
            Status = DocumentStatus.Active,
            EffectiveDate = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        context.Documents.Add(doc);
        await context.SaveChangesAsync();

        var retrieved = await context.Documents.FirstAsync();
        Assert.Equal("Protocolo de Endodoncia", retrieved.Title);
        Assert.Equal(DocumentStatus.Active, retrieved.Status);
    }

    [Fact]
    public async Task CanCreateDocumentWithChunks()
    {
        using var context = CreateInMemoryContext();

        var doc = new Document
        {
            Title = "Guía de Caries",
            SourceType = "PDF",
            OriginalFileName = "guia_caries.pdf",
            Version = "1.0",
            Status = DocumentStatus.Active,
            EffectiveDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            Chunks = new List<DocumentChunk>
            {
                new() { ChunkIndex = 0, Content = "Chunk 1 content", TokenCount = 120 },
                new() { ChunkIndex = 1, Content = "Chunk 2 content", TokenCount = 95 }
            }
        };

        context.Documents.Add(doc);
        await context.SaveChangesAsync();

        var retrieved = await context.Documents
            .Include(d => d.Chunks)
            .FirstAsync();
        Assert.Equal(2, retrieved.Chunks.Count);
    }

    [Fact]
    public async Task CanCreateChatSessionWithMessages()
    {
        using var context = CreateInMemoryContext();

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

        var session = new ChatSession
        {
            Title = "Consulta sobre caries",
            PromptVersionId = prompt.Id,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = "¿Qué es una caries?" },
                new() { Role = "assistant", Content = "Una caries es...", SourceChunkIds = new List<int> { 1, 2 } }
            }
        };

        context.ChatSessions.Add(session);
        await context.SaveChangesAsync();

        var retrieved = await context.ChatSessions
            .Include(s => s.Messages)
            .FirstAsync();
        Assert.Equal(2, retrieved.Messages.Count);
        Assert.Equal(prompt.Id, retrieved.PromptVersionId);
    }

    [Fact]
    public async Task CanCreateAuditLogEntry()
    {
        using var context = CreateInMemoryContext();

        var entry = new AuditLogEntry
        {
            Action = "query",
            RequestPayload = "{\"question\": \"test\"}",
            ResponseSummary = "Test response",
            DurationMs = 250
        };

        context.AuditLog.Add(entry);
        await context.SaveChangesAsync();

        var retrieved = await context.AuditLog.FirstAsync();
        Assert.Equal("query", retrieved.Action);
        Assert.Equal(250, retrieved.DurationMs);
    }
}
