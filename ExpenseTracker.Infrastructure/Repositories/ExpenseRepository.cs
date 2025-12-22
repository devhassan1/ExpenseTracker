using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Persistence;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class ExpenseRepository : IExpenseService
    {
        private readonly OracleDbContext _db;
        public ExpenseRepository(OracleDbContext db) => _db = db;

        public async Task<long> Create(Expense expense, CancellationToken ct)
        {
            await _db.Expenses.AddAsync(expense, ct);
            // UnitOfWork SaveChanges will be called by caller
            return expense.Id;
        }

        public async Task<IReadOnlyList<ExpenseListItem>> ListByDateRange(
            long? userId, DateTime from, DateTime to, CancellationToken ct)
        {
            // Load expenses in range
            var q = _db.Expenses.AsQueryable()
                .Where(e => e.TxnDate >= from && e.TxnDate <= to);

            if (userId is long uid) q = q.Where(e => e.UserId == uid);

            var expenses = await q
                .OrderByDescending(e => e.TxnDate)
                .AsNoTracking()
                .ToListAsync(ct);

            if (expenses.Count == 0) return Array.Empty<ExpenseListItem>();

            // Fetch tag mappings for these expense ids
            var expenseIds = expenses.Select(e => e.Id).ToArray();

            // EXPENSE_TAG(EXPENSE_ID, TAG_ID) and TAGS(ID, LABEL)
            var tags = await _db.Tags
                .Where(t => _db.Database
                    .SqlQueryRaw<long>("SELECT TAG_ID FROM EXPENSE_TAG WHERE EXPENSE_ID = :p0", 0)
                    .ToList()
                    .Contains(t.Id))
                .AsNoTracking()
                .ToListAsync(ct);

            // Simpler: query per expense - not optimal but straightforward
            var result = new List<ExpenseListItem>(expenses.Count);
            foreach (var e in expenses)
            {
                var tagIds = await _db.Database
                    .SqlQueryRaw<long>("SELECT TAG_ID FROM EXPENSE_TAG WHERE EXPENSE_ID = :p0", e.Id)
                    .ToListAsync(ct);

                var tagDtos = _db.Tags
                    .Where(t => tagIds.Contains(t.Id))
                    .AsNoTracking()
                    .Select(t => new TagDto(t.Id, t.Label))
                    .ToArray();

                // Try to get username from USERS table (if present)
                string? userName = null;
                try
                {
                    userName = await _db.Users.Where(u => u.Id == e.UserId).Select(u => u.Name).FirstOrDefaultAsync(ct);
                }
                catch
                {
                    // ignore if Users mapping not present
                }

                result.Add(new ExpenseListItem(
                    e.Id,
                    e.UserId,
                    userName,
                    e.TxnDate,
                    e.Money.Amount,
                    "PKR",
                    e.Description,
                    tagDtos
                ));
            }

            return result;
        }

        
 public async Task<IReadOnlyList<ExpenseListItem>> ListByDateRangePaged(
        long? userId,
        DateTime from,
        DateTime to,
        int page,
        int pageSize,
        string? search,
        CancellationToken ct)
    {
        var q = _db.Expenses.AsQueryable()
            .Where(e => e.TxnDate >= from && e.TxnDate <= to);

        if (userId is long uid)
            q = q.Where(e => e.UserId == uid);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
                // Minimal change: basic contains; if you prefer DB-side LIKE, use EF.Functions.Like
                q = q.Where(e =>
                    e.Description != null && e.Description.Contains(term));
        }

        q = q.OrderByDescending(e => e.TxnDate);

        var size = Math.Clamp(pageSize, 1, 200);
        var pageIndex = Math.Max(page, 1);

        var expenses = await q
            .AsNoTracking()
            .Skip((pageIndex - 1) * size)
            .Take(size)
            .ToListAsync(ct);

        if (expenses.Count == 0)
            return Array.Empty<ExpenseListItem>();

        var result = new List<ExpenseListItem>(expenses.Count);

        foreach (var e in expenses)
        {
            // Per-expense tag lookup (kept same for minimal change)
            var tagIds = await _db.Database
                .SqlQueryRaw<long>("SELECT TAG_ID FROM EXPENSE_TAG WHERE EXPENSE_ID = :p0", e.Id)
                .ToListAsync(ct);

            var tagDtos = await _db.Tags
                .Where(t => tagIds.Contains(t.Id))
                .AsNoTracking()
                .Select(t => new TagDto(t.Id, t.Label))
                .ToArrayAsync(ct);

            // Username (best-effort, as in your original)
            string? userName = null;
            try
            {
                userName = await _db.Users
                    .Where(u => u.Id == e.UserId)
                    .Select(u => u.Name)
                    .FirstOrDefaultAsync(ct);
            }
            catch { /* ignore if Users mapping not present */ }

            result.Add(new ExpenseListItem(
                e.Id,
                e.UserId,
                userName,
                e.TxnDate,
                e.Money.Amount,
                "PKR",
                e.Description,
                tagDtos
            ));
        }

        return result;
    }
}
    }

