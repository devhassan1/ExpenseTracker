using global::ExpenseTracker.Application.Interfaces;
using global::ExpenseTracker.Domain.Entities;
using global::ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class ExpenseRepository : IExpenseRepository
    {
        private readonly OracleDbContext _db;

        public ExpenseRepository(OracleDbContext db) => _db = db;

        public async Task<long> CreateAsync(Expense expense, CancellationToken ct)
        {
         

            await _db.Expenses.AddAsync(expense, ct);
            await _db.SaveChangesAsync(ct);
            return expense.Id;
        }

        public async Task<IReadOnlyList<Expense>> ListByDateRangeAsync(
            long? userId, DateTime from, DateTime to, CancellationToken ct)
        {
            var q = _db.Expenses.AsQueryable()
                .Where(e=> e.TxnDate >= from && e.TxnDate <= to);

            if (userId is long uid) q = q.Where(e => e.UserId == uid);

            // Materialize
            var list = await q
                .OrderByDescending(e => e.TxnDate)
                .AsNoTracking()
                .ToListAsync(ct);

            return list;
        }
    }

}

