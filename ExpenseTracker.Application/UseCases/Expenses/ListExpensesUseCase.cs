using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Application.UseCases.Expenses
{
    public sealed class ListExpensesUseCase
    {
        private readonly IExpenseService _expenses;
        private readonly ICurrentUser _currentUser;

        public ListExpensesUseCase(IExpenseService expenses, ICurrentUser currentUser)
        {
            _expenses = expenses;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<ExpenseListItem>>> ExecuteAsync(
            ExpenseFilterRequest req,
            CancellationToken ct)
        {
            long? targetUserId = req.ForUserId;

            // USER → always only sees their own expenses
            if (_currentUser.Role.Contains("User"))
            {
                targetUserId = _currentUser.UserId;
            }

            // ADMIN → can view any user's expenses except superadmin
            else if (_currentUser.Role.Contains("Admin"))
            {
                // If no specific ForUserId provided → default to own expenses
                if (targetUserId is null)
                    targetUserId = _currentUser.UserId;
            }

            // SUPERADMIN → unrestricted access
            else if (_currentUser.Role.Contains("SuperAdmin"))
            {
                // targetUserId stays as req.ForUserId (null means "all users")
            }

            var items = await _expenses.ListByDateRange(
                targetUserId,
                req.From,
                req.To,
                ct
            );

            return Result<IReadOnlyList<ExpenseListItem>>.Success(items);
        }
    }
}
