using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IExpenseService
    {
        Task<long> Create(Expense expense, CancellationToken ct);
        Task<IReadOnlyList<ExpenseListItem>> ListByDateRange(long? userId, DateTime from, DateTime to, CancellationToken ct);

        Task<IReadOnlyList<ExpenseListItem>> ListByDateRangePaged(
               long? userId,
               DateTime from,
               DateTime to,
               int page,
               int pageSize,
               string? search,
               CancellationToken ct);
    }

}

