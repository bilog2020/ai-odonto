using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class ClaudeLlmServiceTests
{
    [Fact]
    public void BuildPromptWithContext_FormatsCorrectly()
    {
        var chunks = new List<DocumentChunk>
        {
            new() { Id = 1, Content = "La caries es una enfermedad.", Document = new Document { Title = "Guía Caries" } },
            new() { Id = 2, Content = "El tratamiento incluye...", Document = new Document { Title = "Protocolo Endo" } }
        };

        var prompt = ClaudeLlmService.BuildContextPrompt(chunks);

        Assert.Contains("Guía Caries", prompt);
        Assert.Contains("Protocolo Endo", prompt);
        Assert.Contains("La caries es una enfermedad.", prompt);
    }

    [Fact]
    public void BuildContextPrompt_WithNoChunks_ReturnsDisclaimer()
    {
        var chunks = new List<DocumentChunk>();

        var prompt = ClaudeLlmService.BuildContextPrompt(chunks);

        Assert.Contains("No se encontraron documentos relevantes", prompt);
    }
}
