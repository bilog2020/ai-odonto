namespace AiOdonto.Api.Services;

public interface IEmbeddingService
{
    Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts);
}
