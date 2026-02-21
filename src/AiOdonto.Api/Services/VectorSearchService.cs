using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class VectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;
    private readonly IEmbeddingService _embeddingService;

    public VectorSearchService(AppDbContext context, IEmbeddingService embeddingService)
    {
        _context = context;
        _embeddingService = embeddingService;
    }

    public async Task<List<DocumentChunk>> SearchAsync(string query, int topK = 5)
    {
        var queryEmbeddings = await _embeddingService.GenerateEmbeddingsAsync([query]);
        var queryVector = queryEmbeddings[0];

        var chunks = await _context.DocumentChunks
            .Include(c => c.Document)
            .Where(c => c.Embedding != null)
            .ToListAsync();

        return chunks
            .Select(c => (chunk: c, score: CosineSimilarity(queryVector, c.Embedding!)))
            .OrderByDescending(x => x.score)
            .Take(topK)
            .Select(x => x.chunk)
            .ToList();
    }

    private static float CosineSimilarity(float[] a, float[] b)
    {
        float dot = 0, normA = 0, normB = 0;
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }
        return (normA == 0 || normB == 0) ? 0f : dot / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }
}
