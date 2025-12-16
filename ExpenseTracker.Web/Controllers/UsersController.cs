using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;

namespace ExpenseTracker.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController(IUserRepository users, ICurrentUser currentUser) : ControllerBase
{
    private readonly IUserRepository _users = users;
    private readonly ICurrentUser _currentUser = currentUser;

    // GET api/users

    [Authorize] // checks ONLY user's existing role claims
    [HttpGet("loadAllUsers")]
    public async Task<IActionResult> Get(CancellationToken ct = default)
    {
        // SuperAdmin sees all
        if (_currentUser.Role.Contains("SuperAdmin"))
        {
            var all = await _users.ListByParent(null, ct);
            var proj = all.Select(u => new { u.Id, u.Name, u.Email }).ToList();
            return Ok(proj);
        }

        // Admin sees children
        if (_currentUser.Role.Contains("Admin"))
        {
            var list = await _users.ListByParent(_currentUser.UserId, ct);
            var proj = list.Select(u => new { u.Id, u.Name, u.Email }).ToList();
            return Ok(proj);
        }
        return Ok();
    }
}

