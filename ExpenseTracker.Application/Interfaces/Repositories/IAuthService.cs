using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IAuthService
    {
        Task<(bool IsValid, long UserId, string[] Roles)> ValidateCredentialsAsync(
            string username, string password, CancellationToken ct);
        Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct);
        Task<Result<string>> CreateToken(string userId, string username, IEnumerable<string>? roles = null);
    }

    }
