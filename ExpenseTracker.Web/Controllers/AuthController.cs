//using ExpenseTracker.Application.DTOs;
//using ExpenseTracker.Infrastructure.Auth;
//using Microsoft.AspNetCore.Mvc;


//namespace ExpenseTracker.Web.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthController(IAuthService auth, IConfiguration config) : ControllerBase
//{

//    [HttpPost("Login")]
//    public async Task<IActionResult> Login([FromBody] LoginRequest req)
//    {
//        var authResult = await auth.ValidateCredentialsAsync(req.Username, req.Password);
//        if (!authResult.IsValid)
//        {
//            return Unauthorized();
//        }

//        var token = await auth.CreateToken(authResult.UserID, req.Username, authResult.Role);
//        return Ok(new { token });
//    }

//    [HttpPost("register")]
//    public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
//    {
//        var result = await auth.RegisterUser(req, ct);

//        if (!result.IsSuccess)
//            return BadRequest(new { error = result.Error });

//        return Ok(new
//        {
//            user_id = result.Value,
//            message = "User registered successfully"
//        });
//    }
//}

using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Common;       // ICurrentUser
using ExpenseTracker.Application.Interfaces.Repositories; // IUserRepository
using ExpenseTracker.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace ExpenseTracker.Web.Controllers
{
    [ApiController]
    [Route("api/Controller")]

    public sealed class AuthController : ControllerBase
    {
        private readonly ICurrentUser _currentUser;
        private readonly IUserRepository _users;
        private readonly IConfiguration _config;

        public AuthController(ICurrentUser currentUser, IUserRepository users, IConfiguration config)
        {
            _currentUser = currentUser;
            _users = users;
            _config = config;
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.Password) ||
                string.IsNullOrWhiteSpace(req.Username))
                return BadRequest(new { error = "Email, fullName, and password are required." });

            var isAdmin = _currentUser.Role.Contains("Admin");
            var isSuperAdmin = _currentUser.Role.Contains("SuperAdmin");
            if (!(isAdmin || isSuperAdmin)) return Forbid();

            var parentUserId = _currentUser.UserId;

            var adminId = _config.GetValue<int>("Roles:AdminId");
            var userId = _config.GetValue<int>("Roles:UserId");
            var roleId = isSuperAdmin ? adminId : userId;

            var user = new User
            {
                Email = req.Email.Trim(),
                Name = req.Username.Trim(),
                PasswordHash = req.Password,
                parent_user_id = parentUserId,
                RoleId = roleId
            };

            var id = await _users.Create(user, ct);
            return Ok(new { user_id = id, message = $"User created with roleId {roleId} under parent {parentUserId}." });
        }
    }
}