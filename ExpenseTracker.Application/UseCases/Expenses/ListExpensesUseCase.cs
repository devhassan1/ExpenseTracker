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

        private static (DateTime fromDt, DateTime toDt) ToDateRange(DateOnly? from, DateOnly? to)
        {
            var fromDt = from?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue;
            var toDt = to?.ToDateTime(TimeOnly.MaxValue) ?? DateTime.MaxValue;
            return (fromDt, toDt);
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

       

        // NEW: paged + search variant that Facade calls
        public async Task<Result<List<ExpenseListItem>>> GetPagedExpensesAsync(
            ExpenseFilterRequest req,
            int page,
            int pageSize,
            string? search,
            CancellationToken ct)
        {
            long? targetUserId = ResolveTargetUserId(req.ForUserId);

            // Convert DateOnly? → DateTime with safe defaults
            //var (fromDt, toDt) = ToDateRange(req.From,req.To);

            //// Optional guards (avoid crazy sizes)
            var safePage = Math.Max(page, 1);
            var safeSize = Math.Clamp(pageSize, 1, 200);

            var items = await _expenses.ListByDateRangePaged(
                targetUserId,
                req.From,
                req.To,
                safePage,
                safeSize,
                search,
                ct);

            // Convert IReadOnlyList → List to match your Facade signature
            return Result<List<ExpenseListItem>>.Success(items.ToList());
        }

        // --- helpers to keep logic DRY ---
        private long? ResolveTargetUserId(long? forUserId)
        {
            var targetUserId = forUserId;

            if (_currentUser.Role.Contains("User"))
            {
                targetUserId = _currentUser.UserId;
            }
            else if (_currentUser.Role.Contains("Admin"))
            {
                if (targetUserId is null)
                    targetUserId = _currentUser.UserId;
            }
            else if (_currentUser.Role.Contains("SuperAdmin"))
            {
                // unrestricted: keep forUserId as-is (null means "all users")
            }

            return targetUserId;
        }
    }
}

