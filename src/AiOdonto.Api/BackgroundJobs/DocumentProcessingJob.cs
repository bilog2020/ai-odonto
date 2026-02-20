using AiOdonto.Api.Data;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace AiOdonto.Api.BackgroundJobs;

public class DocumentProcessingJob : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DocumentProcessingJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public DocumentProcessingJob(IServiceScopeFactory scopeFactory, ILogger<DocumentProcessingJob> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DocumentProcessingJob started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingDocumentsAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error processing pending documents.");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task ProcessPendingDocumentsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var ingestionService = scope.ServiceProvider.GetRequiredService<DocumentIngestionService>();

        var pending = await context.Documents
            .Where(d => d.Status == DocumentStatus.InReview &&
                        !context.DocumentChunks.Any(c => c.DocumentId == d.Id))
            .Take(5)
            .ToListAsync(ct);

        foreach (var doc in pending)
        {
            _logger.LogInformation("Processing document {DocId}: {Title}", doc.Id, doc.Title);
        }
    }
}
