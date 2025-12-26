//using ExpenseTracker.Application.Interfaces.Common;
//using System.Security.Claims;

//namespace ExpenseTracker.Infrastructure.Auth;

//public sealed class CurrentUser : ICurrentUser
//{
//    public long UserId { get; }
//    public string[] Roles { get; }

//    // Returns the first role for old code compatibility
//    public string Role => Roles?.FirstOrDefault() ?? string.Empty;

//    public CurrentUser(ClaimsPrincipal user)
//    {
//        // Try a number of common claim types that may contain the user id
//        string? idStr = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
//                        ?? user.FindFirst("sub")?.Value
//                        ?? user.FindFirst("nameid")?.Value
//                        ?? user.FindFirst("id")?.Value;

//        UserId = long.TryParse(idStr, out var id) ? id : 0;

//        Roles = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
//    }
//}
using ExpenseTracker.Application.Interfaces.Common;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Auth
{
    public sealed class CurrentUser : ICurrentUser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUser(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public long UserId
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                var idClaim = user?.FindFirst(ClaimTypes.NameIdentifier)
                             ?? user?.FindFirst(ClaimTypes.Name);

                if (idClaim == null)
                    throw new UnauthorizedAccessException("UserId claim not found");

                return long.Parse(idClaim.Value);
            }
        }

        public string Role
        {
            get
            {
                var user = _httpContextAccessor.HttpContext?.User;

                var role = user?.FindFirst(ClaimTypes.Role)?.Value;

                if (string.IsNullOrWhiteSpace(role))
                    throw new UnauthorizedAccessException("Role claim not found");

                return role;
            }
        }
    }
}
