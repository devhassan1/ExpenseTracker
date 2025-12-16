using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ExpenseTracker.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebugController : ControllerBase
{
    // GET /api/debug/headers - returns Authorization header value (no auth required)
    [HttpGet("headers")]
    public IActionResult Headers()
    {
        var auth = Request.Headers["Authorization"].FirstOrDefault();
        return Ok(new { authorization = auth });
    }

    // GET /api/debug/claims - returns authentication status and claims (requires auth)
    [HttpGet("claims")]
    [Authorize]
    public IActionResult Claims()
    {
        var user = HttpContext.User;
        var isAuth = user?.Identity?.IsAuthenticated ?? false;
        var claims = user?.Claims.Select(c => new { c.Type, c.Value }) ?? Enumerable.Empty<object>();
        return Ok(new { isAuthenticated = isAuth, claims = claims });
    }
}
