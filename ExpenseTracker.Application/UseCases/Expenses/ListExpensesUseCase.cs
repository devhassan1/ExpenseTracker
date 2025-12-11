//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ExpenseTracker.Application.UseCases.Expenses
//{

//    using ExpenseTracker.Application.DTOs;
//    using ExpenseTracker.Application.Interfaces;
//    using ExpenseTracker.Common.Results;
//    using ExpenseTracker.Domain.Entities;

//    public sealed class ListExpensesUseCase
//    {
//        private readonly IExpenseRepository _expenses;
//        private readonly ICurrentUser _currentUser;

//        public ListExpensesUseCase(IExpenseRepository expenses, ICurrentUser currentUser)
//        {
//            _expenses = expenses;
//            _currentUser = currentUser;
//        }

//        public async Task<Result<IReadOnlyList<Expense>>> ExecuteAsync(ExpenseFilterRequest req, CancellationToken ct)
//        {
//            long? targetUserId = req.ForUserId;

//            // RBAC:
//            // - User: can only view own (ignore ForUserId)
//            // - Admin: can view users in their org (we assume infra filters by orgId)
//            // - SuperAdmin: can view across all orgs (we’ll pass orgId; infra may override in special repo method)

//            if (_currentUser.Role.Contains("User"))) targetUserId = _currentUser.UserId;

//            var items = await _expenses.ListByDateRangeAsync(targetUserId,req.From,req.To,ct);

//            return Result<IReadOnlyList<Expense>>.Success(items);
//        }
//    }

//}

using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.UseCases.Expenses
{
    public sealed class ListExpensesUseCase
    {
        private readonly IExpenseRepository _expenses;
        private readonly ICurrentUser _currentUser;

        public ListExpensesUseCase(IExpenseRepository expenses, ICurrentUser currentUser)
        {
            _expenses = expenses;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<Expense>>> ExecuteAsync(
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

            var items = await _expenses.ListByDateRangeAsync(
                targetUserId,
                req.From,
                req.To,
                ct
            );

            return Result<IReadOnlyList<Expense>>.Success(items);
        }
    }
}
