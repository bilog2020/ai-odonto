using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class PromptServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreatePrompt_DefaultsToDraft()
    {
        using var context = CreateContext();
        var service = new PromptService(context);

        var prompt = await service.CreateAsync("v1.0", "You are a dental AI.", "admin", "Initial prompt");

        Assert.Equal(PromptStatus.Draft, prompt.Status);
        Assert.Null(prompt.ActivatedAt);
    }

    [Fact]
    public async Task ActivatePrompt_DeactivatesOtherProductionPrompts()
    {
        using var context = CreateContext();
        var service = new PromptService(context);

        var existing = await service.CreateAsync("v1.0", "Old prompt", "admin", "First");
        await service.ActivateAsync(existing.Id);

        var newPrompt = await service.CreateAsync("v2.0", "New prompt", "admin", "Second");
        await service.ActivateAsync(newPrompt.Id);

        var reloaded = await context.PromptVersions.FindAsync(existing.Id);
        Assert.Equal(PromptStatus.Draft, reloaded!.Status);

        var active = await service.GetActiveAsync();
        Assert.Equal(newPrompt.Id, active!.Id);
        Assert.Equal(PromptStatus.Production, active.Status);
        Assert.NotNull(active.ActivatedAt);
    }

    [Fact]
    public async Task GetActivePrompt_ReturnsProductionPrompt()
    {
        using var context = CreateContext();
        var service = new PromptService(context);

        var prompt = await service.CreateAsync("v1.0", "System prompt", "admin", "First prompt");
        await service.ActivateAsync(prompt.Id);

        var active = await service.GetActiveAsync();

        Assert.NotNull(active);
        Assert.Equal(prompt.Id, active.Id);
        Assert.Equal(PromptStatus.Production, active.Status);
    }
}
