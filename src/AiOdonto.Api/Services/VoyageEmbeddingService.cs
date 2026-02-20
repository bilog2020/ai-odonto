using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace AiOdonto.Api.Services;

public class VoyageEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;

    public VoyageEmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<float[]>> GenerateEmbeddingsAsync(List<string> texts)
    {
        var request = new
        {
            input = texts,
            model = "voyage-3"
        };

        var response = await _httpClient.PostAsJsonAsync("v1/embeddings", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<VoyageEmbeddingResponse>();

        return result!.Data
            .OrderBy(d => d.Index)
            .Select(d => d.Embedding)
            .ToList();
    }
}

internal class VoyageEmbeddingResponse
{
    [JsonPropertyName("data")]
    public List<VoyageEmbeddingData> Data { get; set; } = new();
}

internal class VoyageEmbeddingData
{
    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("embedding")]
    public float[] Embedding { get; set; } = Array.Empty<float>();
}
