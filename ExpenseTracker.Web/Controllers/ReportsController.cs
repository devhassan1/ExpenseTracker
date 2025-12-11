using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Web.Facades;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly ReportFacade _facade;

    public ReportsController(ReportFacade facade)
    {
        _facade = facade;
    }

    // GET: api/reports/export
    [HttpGet("export")]
    public async Task<IActionResult> ExportAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] long? forUserId,
        [FromQuery] string format = "csv",
        CancellationToken ct = default)
    {
        var req = new ExportRequest(from, to, forUserId, format);

        var result = await _facade.ExportAsync(req, ct);

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Error });

        var fileBytes = result.Value;
        var mime = format.ToLower() switch
        {
            "xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "csv" => "text/csv",
            "pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

        var filename = $"expenses_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";
        return File(fileBytes, mime, filename);
    }
}
