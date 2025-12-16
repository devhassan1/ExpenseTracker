using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Infrastructure.Auth;

public interface IAuthService
{
    Task<LoginResponseModel> ValidateCredentialsAsync(string username, string password);
    Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct);
    Task<string> CreateToken(int userId, string username, string? role = null);
}
