
// src/ExpenseTracker.Web/Controllers/TagsController.cs
using ExpenseTracker.Application.UseCases;
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
    public async Task<ActionResult<List<Tag>>> Get([FromQuery] string? label, [FromQuery] bool includeGlobal = true, CancellationToken ct = default)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                var tag = await _service.GetByLabel(label, ct);
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

    // POST api/tags
    [HttpPost]
    public async Task<ActionResult<long>> Create([FromBody] Tag tag, CancellationToken ct)
    {
        try
        {
            var id = await _service.Create(tag, ct);
            return Ok(id);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create tag", detail = ex.Message });
        }
    }
}
