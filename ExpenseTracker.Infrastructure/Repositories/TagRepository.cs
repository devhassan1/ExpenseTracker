using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Entities.ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly IRepository<Tag> _repo;
    private readonly OracleDbContext _db;

    public TagRepository(IRepository<Tag> repo, OracleDbContext db)
    {
        _repo = repo;
        _db = db;
    }

    public Task<Tag?> GetByLabel(string label, CancellationToken ct)
        => _repo.GetById(0, ct); // placeholder: domain-specific lookup remains using Query in implementations

    public async Task<long> Create(Tag tag, CancellationToken ct)
    {
        await _repo.Add(tag, ct);
        // UnitOfWork SaveChanges will be called by caller
        return tag.Id;
    }

    public async Task<List<Tag>> ListByCategory(long categoryId, CancellationToken ct)
    {
        // Adjust table/column names if different in your Oracle schema
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
        return await _db.Tags
            .AsNoTracking()
            .ToListAsync(ct);
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

}
