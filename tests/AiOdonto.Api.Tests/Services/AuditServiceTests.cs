using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Tests.Services;

public class AuditServiceTests
{
    private AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task LogQuery_CreatesAuditEntry()
    {
        using var context = CreateContext();
        var service = new AuditService(context);

        await service.LogQueryAsync(
            userId: "user-1",
            sessionId: 1,
            promptVersionId: 1,
            retrievedChunkIds: new List<int> { 10, 20 },
            request: "¿Qué es la caries?",
            response: "La caries es...",
            durationMs: 350
        );

        var entry = await context.AuditLog.FirstAsync();
        Assert.Equal("query", entry.Action);
        Assert.Equal("user-1", entry.UserId);
        Assert.Equal(350, entry.DurationMs);
        Assert.Equal(2, entry.RetrievedChunkIds.Count);
    }
}
