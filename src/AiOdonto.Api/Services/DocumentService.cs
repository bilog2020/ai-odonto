using Microsoft.EntityFrameworkCore;
using AiOdonto.Api.Data;
using AiOdonto.Api.Models;

namespace AiOdonto.Api.Services;

public class DocumentService
{
    private readonly AppDbContext _context;

    public DocumentService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Document> CreateDocumentAsync(
        string title, string sourceType, string originalFileName,
        string version, DateTime effectiveDate, string uploadedBy)
    {
        var doc = new Document
        {
            Title = title,
            SourceType = sourceType,
            OriginalFileName = originalFileName,
            Version = version,
            Status = DocumentStatus.InReview,
            EffectiveDate = effectiveDate,
            UploadedBy = uploadedBy
        };

        _context.Documents.Add(doc);
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task<List<Document>> GetDocumentsAsync(DocumentStatus? status = null)
    {
        var query = _context.Documents.AsQueryable();
        if (status.HasValue)
            query = query.Where(d => d.Status == status.Value);
        return await query.OrderByDescending(d => d.CreatedAt).ToListAsync();
    }

    public async Task<Document?> GetDocumentByIdAsync(int id)
    {
        return await _context.Documents
            .Include(d => d.Chunks.OrderBy(c => c.ChunkIndex))
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Document?> UpdateStatusAsync(int id, DocumentStatus status)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null) return null;

        doc.Status = status;
        doc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return doc;
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var doc = await _context.Documents.FindAsync(id);
        if (doc == null) return false;

        doc.Status = DocumentStatus.Deprecated;
        doc.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}
