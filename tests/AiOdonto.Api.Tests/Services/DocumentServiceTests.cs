using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class DocumentServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateDocument_ReturnsDocumentWithId()
    {
        using var context = CreateContext();
        var service = new DocumentService(context);

        var doc = await service.CreateDocumentAsync(
            title: "Protocolo Endodoncia",
            sourceType: "PDF",
            originalFileName: "proto_endo.pdf",
            version: "1.0",
            effectiveDate: new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            uploadedBy: "user-123"
        );

        Assert.True(doc.Id > 0);
        Assert.Equal("Protocolo Endodoncia", doc.Title);
        Assert.Equal(DocumentStatus.InReview, doc.Status);
    }

    [Fact]
    public async Task GetDocuments_ReturnsFilteredByStatus()
    {
        using var context = CreateContext();
        var service = new DocumentService(context);

        await service.CreateDocumentAsync("Doc 1", "PDF", "doc1.pdf", "1.0",
            DateTime.UtcNow, "user-1");
        var doc2 = await service.CreateDocumentAsync("Doc 2", "PDF", "doc2.pdf", "1.0",
            DateTime.UtcNow, "user-1");
        await service.UpdateStatusAsync(doc2.Id, DocumentStatus.Active);

        var activeDocs = await service.GetDocumentsAsync(status: DocumentStatus.Active);

        Assert.Single(activeDocs);
        Assert.Equal("Doc 2", activeDocs[0].Title);
    }

    [Fact]
    public async Task UpdateStatus_ChangesDocumentStatus()
    {
        using var context = CreateContext();
        var service = new DocumentService(context);

        var doc = await service.CreateDocumentAsync("Test Doc", "PDF", "test.pdf", "1.0",
            DateTime.UtcNow, "user-1");

        var updated = await service.UpdateStatusAsync(doc.Id, DocumentStatus.Active);

        Assert.Equal(DocumentStatus.Active, updated!.Status);
    }

    [Fact]
    public async Task GetDocumentById_IncludesChunks()
    {
        using var context = CreateContext();
        var service = new DocumentService(context);

        var doc = await service.CreateDocumentAsync("Test Doc", "PDF", "test.pdf", "1.0",
            DateTime.UtcNow, "user-1");

        context.DocumentChunks.AddRange(
            new DocumentChunk { DocumentId = doc.Id, ChunkIndex = 0, Content = "Chunk 0", TokenCount = 50 },
            new DocumentChunk { DocumentId = doc.Id, ChunkIndex = 1, Content = "Chunk 1", TokenCount = 60 }
        );
        await context.SaveChangesAsync();

        var retrieved = await service.GetDocumentByIdAsync(doc.Id);

        Assert.NotNull(retrieved);
        Assert.Equal(2, retrieved!.Chunks.Count);
    }
}
