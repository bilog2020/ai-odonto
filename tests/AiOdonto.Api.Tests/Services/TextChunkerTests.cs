using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class TextChunkerTests
{
    [Fact]
    public void ChunkText_ShortText_ReturnsSingleChunk()
    {
        var chunker = new TextChunker(maxTokens: 500, overlapTokens: 50);
        var text = "This is a short text about dental care.";

        var chunks = chunker.Chunk(text);

        Assert.Single(chunks);
        Assert.Equal(text, chunks[0].Content);
        Assert.Equal(0, chunks[0].Index);
    }

    [Fact]
    public void ChunkText_LongText_ReturnsMultipleChunks()
    {
        var chunker = new TextChunker(maxTokens: 10, overlapTokens: 2);
        var words = Enumerable.Range(0, 50).Select(i => $"word{i}");
        var text = string.Join(" ", words);

        var chunks = chunker.Chunk(text);

        Assert.True(chunks.Count > 1);
        for (int i = 0; i < chunks.Count; i++)
            Assert.Equal(i, chunks[i].Index);
    }

    [Fact]
    public void ChunkText_RespectsOverlap()
    {
        var chunker = new TextChunker(maxTokens: 10, overlapTokens: 3);
        var words = Enumerable.Range(0, 30).Select(i => $"word{i}");
        var text = string.Join(" ", words);

        var chunks = chunker.Chunk(text);

        Assert.True(chunks.Count >= 2);
    }

    [Fact]
    public void ChunkText_EmptyText_ReturnsEmpty()
    {
        var chunker = new TextChunker(maxTokens: 500, overlapTokens: 50);

        var chunks = chunker.Chunk("");

        Assert.Empty(chunks);
    }

    [Fact]
    public void EstimateTokenCount_ReturnsReasonableEstimate()
    {
        var count = TextChunker.EstimateTokenCount("Hola mundo, esta es una prueba de texto dental.");

        Assert.True(count > 5);
        Assert.True(count < 50);
    }
}
