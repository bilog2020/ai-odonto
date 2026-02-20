namespace AiOdonto.Api.Services;

public class TextChunker
{
    private readonly int _maxTokens;
    private readonly int _overlapTokens;

    public TextChunker(int maxTokens = 500, int overlapTokens = 50)
    {
        _maxTokens = maxTokens;
        _overlapTokens = overlapTokens;
    }

    public List<ChunkResult> Chunk(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new List<ChunkResult>();

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<ChunkResult>();
        int position = 0;
        int index = 0;

        while (position < words.Length)
        {
            var chunkWords = new List<string>();
            int tokenCount = 0;
            int i = position;

            while (i < words.Length && tokenCount < _maxTokens)
            {
                chunkWords.Add(words[i]);
                tokenCount += EstimateWordTokens(words[i]);
                i++;
            }

            var content = string.Join(" ", chunkWords);
            chunks.Add(new ChunkResult
            {
                Index = index,
                Content = content,
                TokenCount = EstimateTokenCount(content)
            });

            // If we consumed all remaining words, we're done
            if (i >= words.Length)
                break;

            // Move position forward, accounting for overlap
            int wordsToAdvance = chunkWords.Count;
            int overlapWords = 0;
            int overlapTokenCount = 0;

            for (int j = chunkWords.Count - 1; j >= 0 && overlapTokenCount < _overlapTokens; j--)
            {
                overlapTokenCount += EstimateWordTokens(chunkWords[j]);
                overlapWords++;
            }

            position += Math.Max(1, wordsToAdvance - overlapWords);
            index++;
        }

        return chunks;
    }

    public static int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text)) return 0;
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private static int EstimateWordTokens(string word)
    {
        return word.Length > 12 ? 2 : 1;
    }
}

public class ChunkResult
{
    public int Index { get; set; }
    public string Content { get; set; } = string.Empty;
    public int TokenCount { get; set; }
}
