using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class DocumentIngestionService
{
    private readonly AppDbContext _context;
    private readonly TextChunker _chunker;
    private readonly IEmbeddingService _embeddingService;

    public DocumentIngestionService(
        AppDbContext context,
        TextChunker chunker,
        IEmbeddingService embeddingService)
    {
        _context = context;
        _chunker = chunker;
        _embeddingService = embeddingService;
    }

    public async Task ProcessTextAsync(int documentId, string text)
    {
        var chunkResults = _chunker.Chunk(text);
        if (chunkResults.Count == 0) return;

        var contents = chunkResults.Select(c => c.Content).ToList();
        var embeddings = await _embeddingService.GenerateEmbeddingsAsync(contents);

        var chunks = chunkResults.Select((c, i) => new DocumentChunk
        {
            DocumentId = documentId,
            ChunkIndex = c.Index,
            Content = c.Content,
            Embedding = embeddings[i],
            TokenCount = c.TokenCount
        }).ToList();

        _context.DocumentChunks.AddRange(chunks);
        await _context.SaveChangesAsync();
    }
}
