
// src/ExpenseTracker.Web/Controllers/TagsController.cs
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;

    public TagsController(ITagService service) => _service = service;

    // GET api/tags?label=&includeGlobal=true
    [HttpGet]
    public async Task<ActionResult<List<Tag>>> GetAsync([FromQuery] string? label, [FromQuery] bool includeGlobal = true, CancellationToken ct = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                var tag = await _service.GetByLabelAsync(label, ct);
                return Ok(tag is null ? new List<Tag>() : new List<Tag> { tag });
            }

            var list = await _service.GetAllAsync(ct);
            return Ok(list.ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to load tags", detail = ex.Message });
        }
    }

    // POST api/tags
    [HttpPost]
    public async Task<ActionResult<long>> CreateAsync([FromBody] Tag tag, CancellationToken ct)
    {
        try
        {
            var id = await _service.CreateAsync(tag, ct);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create tag", detail = ex.Message });
        }
    }
}
