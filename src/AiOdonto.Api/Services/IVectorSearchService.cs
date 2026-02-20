using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public interface IVectorSearchService
{
    Task<List<DocumentChunk>> SearchAsync(string query, int topK = 5);
}
