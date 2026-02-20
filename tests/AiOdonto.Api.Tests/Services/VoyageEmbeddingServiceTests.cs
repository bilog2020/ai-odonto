using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class VoyageEmbeddingServiceTests
{
    [Fact]
    public async Task GenerateEmbeddings_ReturnsParsedVectors()
    {
        var mockResponse = new
        {
            data = new[]
            {
                new { embedding = new float[] { 0.1f, 0.2f, 0.3f } },
                new { embedding = new float[] { 0.4f, 0.5f, 0.6f } }
            }
        };

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(mockResponse))
            });

        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.voyageai.com/")
        };

        var service = new VoyageEmbeddingService(httpClient);

        var embeddings = await service.GenerateEmbeddingsAsync(
            new List<string> { "text one", "text two" });

        Assert.Equal(2, embeddings.Count);
        Assert.Equal(3, embeddings[0].Length);
        Assert.Equal(0.1f, embeddings[0][0]);
    }
}
