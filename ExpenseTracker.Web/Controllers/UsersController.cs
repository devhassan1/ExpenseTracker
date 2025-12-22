using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;

namespace ExpenseTracker.Web.Controllers;
[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class UsersController(IUserRepository users, ICurrentUser currentUser) : ControllerBase
{
    private readonly IUserRepository _users = users;
    private readonly ICurrentUser _currentUser = currentUser;

    [HttpGet]
    public async Task<IActionResult> Get(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20,
    [FromQuery] string? search = null,
    CancellationToken ct = default)
    {
        if (_currentUser.Role.Contains("SuperAdmin"))
        {
            var all = await _users.ListByParentPaged(null, page, pageSize, search, ct);
            return Ok(all.Select(u => new { u.Id, u.Name, u.Email }));
        }

        if (_currentUser.Role.Contains("Admin"))
        {
            var list = await _users.ListByParentPaged(_currentUser.UserId, page, pageSize, search, ct);
            return Ok(list.Select(u => new { u.Id, u.Name, u.Email }));
        }

        return Forbid();
    }

}
