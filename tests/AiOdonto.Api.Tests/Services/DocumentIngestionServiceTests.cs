using Microsoft.EntityFrameworkCore;
using Moq;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class DocumentIngestionServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task ProcessText_CreatesChunksWithEmbeddingPlaceholders()
    {
        using var context = CreateContext();
        var embeddingServiceMock = new Mock<IEmbeddingService>();
        embeddingServiceMock
            .Setup(x => x.GenerateEmbeddingsAsync(It.IsAny<List<string>>()))
            .ReturnsAsync((List<string> texts) =>
                texts.Select(_ => new float[1536]).ToList());

        var service = new DocumentIngestionService(context, new TextChunker(), embeddingServiceMock.Object);

        var doc = new Document
        {
            Title = "Test Doc",
            SourceType = "TXT",
            OriginalFileName = "test.txt",
            Version = "1.0",
            Status = DocumentStatus.InReview,
            EffectiveDate = DateTime.UtcNow
        };
        context.Documents.Add(doc);
        await context.SaveChangesAsync();

        var text = "This is sample text about dental procedures. " +
                   "It covers various topics related to odontology.";

        await service.ProcessTextAsync(doc.Id, text);

        var chunks = await context.DocumentChunks
            .Where(c => c.DocumentId == doc.Id)
            .ToListAsync();

        Assert.NotEmpty(chunks);
        Assert.All(chunks, c => Assert.NotNull(c.Embedding));
        Assert.All(chunks, c => Assert.True(c.TokenCount > 0));
    }
}
