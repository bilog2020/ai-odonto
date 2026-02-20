using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize]
public class DocumentsController : ControllerBase
{
    private readonly DocumentService _documentService;

    public DocumentsController(DocumentService documentService)
    {
        _documentService = documentService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDocuments([FromQuery] DocumentStatus? status)
    {
        var docs = await _documentService.GetDocumentsAsync(status);
        return Ok(docs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        var doc = await _documentService.GetDocumentByIdAsync(id);
        if (doc == null) return NotFound();
        return Ok(doc);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = "Faculty,Admin")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusRequest request)
    {
        var doc = await _documentService.UpdateStatusAsync(id, request.Status);
        if (doc == null) return NotFound();
        return Ok(doc);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _documentService.SoftDeleteAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}

public class UpdateStatusRequest
{
    public DocumentStatus Status { get; set; }
}
