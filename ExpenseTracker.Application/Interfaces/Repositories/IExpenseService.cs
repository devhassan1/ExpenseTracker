using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IExpenseService
    {
        Task<long> Create(Expense expense, CancellationToken ct);
        Task<IReadOnlyList<DTOs.ExpenseListItem>> ListByDateRange(long? userId, DateTime from, DateTime to, CancellationToken ct);
    }
}
