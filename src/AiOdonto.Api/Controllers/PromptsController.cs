using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AiOdonto.Api.Models;
using AiOdonto.Api.Services;

namespace AiOdonto.Api.Controllers;

[ApiController]
[Route("api/prompts")]
[Authorize(Roles = "Faculty,Admin")]
public class PromptsController : ControllerBase
{
    private readonly PromptService _promptService;

    public PromptsController(PromptService promptService)
    {
        _promptService = promptService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var prompts = await _promptService.GetAllAsync();
        return Ok(prompts);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var prompt = await _promptService.GetByIdAsync(id);
        return prompt is null ? NotFound() : Ok(prompt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePromptRequest request)
    {
        var prompt = await _promptService.CreateAsync(
            request.Version, request.SystemPrompt, request.Author, request.Description);
        return Ok(prompt);
    }

    [HttpPut("{id:int}/activate")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Activate(int id)
    {
        try
        {
            var prompt = await _promptService.ActivateAsync(id);
            return Ok(prompt);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}

public record CreatePromptRequest(string Version, string SystemPrompt, string Author, string Description);
