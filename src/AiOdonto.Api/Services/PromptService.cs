using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class PromptService
{
    private readonly AppDbContext _context;

    public PromptService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PromptVersion>> GetAllAsync()
        => await _context.PromptVersions.OrderByDescending(p => p.CreatedAt).ToListAsync();

    public async Task<PromptVersion?> GetByIdAsync(int id)
        => await _context.PromptVersions.FindAsync(id);

    public async Task<PromptVersion?> GetActiveAsync()
        => await _context.PromptVersions.FirstOrDefaultAsync(p => p.Status == PromptStatus.Production);

    public async Task<PromptVersion> CreateAsync(string version, string systemPrompt, string author, string description)
    {
        var prompt = new PromptVersion
        {
            Version = version,
            SystemPrompt = systemPrompt,
            Author = author,
            Description = description,
            Status = PromptStatus.Draft
        };

        _context.PromptVersions.Add(prompt);
        await _context.SaveChangesAsync();
        return prompt;
    }

    public async Task<PromptVersion> ActivateAsync(int id)
    {
        var current = await _context.PromptVersions
            .FirstOrDefaultAsync(p => p.Status == PromptStatus.Production);

        if (current != null)
            current.Status = PromptStatus.Draft;

        var target = await _context.PromptVersions.FindAsync(id)
            ?? throw new KeyNotFoundException($"PromptVersion {id} not found.");

        target.Status = PromptStatus.Production;
        target.ActivatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return target;
    }
}
