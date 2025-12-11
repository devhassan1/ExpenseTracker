using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IExpenseRepository
    {
        Task<long> CreateAsync(Expense expense, CancellationToken ct);
        Task<IReadOnlyList<Expense>> ListByDateRangeAsync(long? userId, DateTime from, DateTime to, CancellationToken ct);
    }
}
