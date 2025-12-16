using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Infrastructure.Auth;
using Microsoft.AspNetCore.Mvc;


namespace ExpenseTracker.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService auth, IConfiguration config) : ControllerBase
{

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest req)
    {
        var authResult = await auth.ValidateCredentialsAsync(req.Username, req.Password);
        if (!authResult.IsValid)
        {
            return Unauthorized();
        }

        var token = await auth.CreateToken(authResult.UserID,req.Username,authResult.Role);
        return Ok(new { token });
    }
}
