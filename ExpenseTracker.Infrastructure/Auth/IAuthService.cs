using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Auth;

public interface IAuthService
{
    Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
        string username, string password, CancellationToken ct);
}

