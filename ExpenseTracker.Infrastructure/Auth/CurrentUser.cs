//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//using ExpenseTracker.Application.Interfaces;
//using System.Security.Claims;

//namespace ExpenseTracker.Infrastructure.Auth
//{
//    public sealed class CurrentUser : ICurrentUser
//    {
//        public long UserId { get; }
//        public long OrganizationId { get; }
//        public string Role { get; }

//        public CurrentUser(ClaimsPrincipal user)
//        {
//            UserId = long.TryParse(user.FindFirst("sub")?.Value, out var uid) ? uid : 0;
//            OrganizationId = long.TryParse(user.FindFirst("org")?.Value, out var oid) ? oid : 0;
//            Role = user.FindFirst(ClaimTypes.Role)?.Value ?? "User";
//        }
//    }
//}
using ExpenseTracker.Application.Interfaces;
using System.Security.Claims;

namespace ExpenseTracker.Infrastructure.Auth;

public sealed class CurrentUser : ICurrentUser
{
    public long UserId { get; }
    public string[] Roles { get; }

    // Returns the first role for old code compatibility
    public string Role => Roles?.FirstOrDefault() ?? string.Empty;

    public CurrentUser(ClaimsPrincipal user)
    {
        UserId = long.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

        Roles = user.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray();
    }
}
