
// ExpenseTracker.Infrastructure.Repositories/TagRepository.cs
using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Entities.ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class TagRepository : ITagRepository
    {
        private readonly IRepository<Tag> _repo;
        private readonly OracleDbContext _db;

        public TagRepository(IRepository<Tag> repo, OracleDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        // Case-insensitive exact match on LOWER(TRIM(LABEL))
        public async Task<Tag?> GetByLabel(string label, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(label)) return null;
            var norm = label.Trim().ToLowerInvariant();

            // Reliable Oracle translation via raw SQL for exact match
            var sql = "SELECT * FROM TAGS WHERE LOWER(TRIM(LABEL)) = :p0 FETCH FIRST 1 ROWS ONLY";
            return await _db.Tags.FromSqlRaw(sql, norm)
                                 .AsNoTracking()
                                 .FirstOrDefaultAsync(ct);
        }

        public async Task<long> Create(Tag tag, CancellationToken ct)
        {
            await _repo.Add(tag, ct); // UnitOfWork SaveChanges will be called by caller
            return tag.Id;
        }

        public async Task<List<Tag>> ListByCategory(long categoryId, CancellationToken ct)
        {
            var tagIds = await _db.Database
                .SqlQueryRaw<long>("SELECT TAG_ID FROM CATEGORY_TAG WHERE CATEGORY_ID = :p0", categoryId)
                .ToListAsync(ct);

            return await _db.Tags
                .AsNoTracking()
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync(ct);
        }

        public Task AssignToExpense(long expenseId, long tagId, CancellationToken ct)
        {
            _db.ExpenseTags.Add(new ExpenseTag { ExpenseId = expenseId, TagId = tagId });
            return Task.CompletedTask;
        }

        public async Task<List<Tag>> ListAll(CancellationToken ct)
        {
            return await _db.Tags.AsNoTracking().OrderBy(t => t.Label).ToListAsync(ct);
        }

        public Task AddLinksForExpenseAsync(Expense expense, IEnumerable<long> tagIds, CancellationToken ct)
        {
            foreach (var tId in tagIds ?? Enumerable.Empty<long>())
            {
                if (tId <= 0) continue;
                _db.ExpenseTags.Add(new ExpenseTag
                {
                    Expense = expense, // navigation; EF will set ExpenseId on save
                    TagId = tId
                });
            }
            return Task.CompletedTask;
        }

        // NEW: partial, case-insensitive search using LOWER(LABEL) LIKE %q%
        public async Task<IReadOnlyList<Tag>> SearchAsync(string q, int limit, CancellationToken ct)
        {
            q = (q ?? string.Empty).Trim();
            if (q.Length == 0) return Array.Empty<Tag>();

            var qLower = q.ToLowerInvariant();
            // Prefer LINQ to let EF/Oracle provider build the LIKE and FETCH FIRST query safely
            return await _db.Tags
                .AsNoTracking()
                 .Where(t => EF.Functions.Like(t.Label.ToLower(), $"%{qLower}%"))
                .OrderBy(t => t.Label)
                .Take(Math.Clamp(limit, 1, 50))
                .ToListAsync(ct);
        }
    }
}
