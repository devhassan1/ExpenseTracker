
// src/ExpenseTracker.Infrastructure/Repositories/TagRepository.cs
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly OracleDbContext _db;

    public TagRepository(OracleDbContext db) => _db = db;

    public Task<Tag?> GetByLabelAsync(string label, CancellationToken ct)
        => _db.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Label == label, ct);

    public async Task<long> CreateAsync(Tag tag, CancellationToken ct)
    {
        await _db.Tags.AddAsync(tag, ct);
        await _db.SaveChangesAsync(ct);
        return tag.Id;
    }

    public async Task<List<Tag>> ListByCategoryAsync(long categoryId, CancellationToken ct)
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

    public Task AssignToExpenseAsync(long expenseId, long tagId, CancellationToken ct)
        => _db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO EXPENSE_TAG (EXPENSE_ID, TAG_ID) VALUES ({expenseId}, {tagId})", ct);

    public async Task<List<Tag>> ListAllAsync(CancellationToken ct)
    {
        return await _db.Tags
            .AsNoTracking()
            .ToListAsync(ct);
    }
}
