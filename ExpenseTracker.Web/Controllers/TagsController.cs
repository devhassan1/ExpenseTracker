
// src/ExpenseTracker.Web/Controllers/TagsController.cs
using ExpenseTracker.Application.UseCases;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using Tag = ExpenseTracker.Domain.Entities.Tag;

namespace ExpenseTracker.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;
    public TagsController(ITagService service) => _service = service;

    // GET api/tags?label=&includeGlobal=true
    // Existing behavior: if label is specified, returns a single tag (exact match); else all.
    [HttpGet]
    public async Task<ActionResult<List<Tag>>> Get(
        [FromQuery] string? label,
        [FromQuery] bool includeGlobal = true,
        CancellationToken ct = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                var tag = await _service.GetByLabel(label.Trim(), ct);
                return Ok(tag is null ? new List<Tag>() : new List<Tag> { tag });
            }

            var list = await _service.GetAll(ct);
            return Ok(list.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to load tags", detail = ex.Message });
        }
    }

    // NEW: GET api/tags/search?q=Fuel&limit=15
    // Typeahead search (case-insensitive, partial match). Returns TagDto list.
    [HttpGet("search")]
    public async Task<ActionResult<List<TagDto>>> Search(
        [FromQuery] string q,
        [FromQuery] int limit = 15,
        CancellationToken ct = default)
    {
        try
        {
            q = (q ?? string.Empty).Trim();
            if (q.Length == 0)
                return Ok(Array.Empty<TagDto>());

            var results = await _service.SearchAsync(q, Math.Clamp(limit, 1, 50), ct);
            var dtos = results.Select(t => new TagDto(t.Id, t.Label)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to search tags", detail = ex.Message });
        }
    }

    public sealed record CreateTagRequest(string Label);

    // POST api/tags
    // Create a tag if it doesn't exist (case-insensitive exact match on LOWER(TRIM(LABEL)).
    // Returns 201 with TagDto; returns 409 if already exists; returns 422 if invalid.
    [HttpPost]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateTagRequest req, CancellationToken ct)
    {
        try
        {
            var label = (req?.Label ?? string.Empty).Trim();
            if (label.Length == 0 || label.Length > 100)
                return UnprocessableEntity(new { error = "Invalid tag label" });

            var existing = await _service.GetByLabel(label, ct);
            if (existing is not null)
                return Conflict(new { error = "Tag already exists" });

            var id = await _service.Create(new Tag { Label = label }, ct);
            var created = new TagDto(id, label);
            return Created($"/api/tags/{id}", created);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create tag", detail = ex.Message });
        }
    }
}
