using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IAuthService
    {
        Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
            string username, string password, CancellationToken ct);

        // Optional: let Auth orchestrate registration via repository abstraction.
        Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct);
        Task<Result<string>> CreateToken(string userId, string username, IEnumerable<string>? roles = null);
    }

    }
