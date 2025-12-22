using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Web.Facades;
using Microsoft.AspNetCore.Mvc;
using System;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly ExpenseFacade _facade;

    public ExpensesController(ExpenseFacade facade)
    {
        _facade = facade;
    }

    // POST: api/expenses
    [HttpPost]
    public async Task<IActionResult> AddAsync([FromBody] AddExpenseRequest request, CancellationToken ct)
    {
        var result = await _facade.AddAsync(request, ct);


        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(new { id = result.Value });
    }

    // GET: api/expenses

    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? forUserId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var req = new ExpenseFilterRequest(from, to, forUserId);

        var result = await _facade.ListAsync(req, page, pageSize, search, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        return Ok(result.Value); // Will now return a limited list
    }

}
