using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByName(string name);
        Task<string> GetRoleByID(long id);
        Task<long> Create(User user, CancellationToken ct);
        Task<IReadOnlyList<User>> ListByParent(long? parentUserId, CancellationToken ct);
        Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct);
    }
}
