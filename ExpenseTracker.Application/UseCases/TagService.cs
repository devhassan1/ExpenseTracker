
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Application.Interfaces.Services;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Services;

public class TagService : ITagService
{
    private readonly ITagRepository _repo;

    public TagService(ITagRepository repo) => _repo = repo;

    public Task<List<Tag>> GetAllAsync(CancellationToken ct) => _repo.ListAllAsync(ct);

    public Task<Tag?> GetByLabelAsync(string label, CancellationToken ct) => _repo.GetByLabelAsync(label, ct);

    public Task<long> CreateAsync(Tag tag, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tag.Label))
            throw new ArgumentException("Label is required.", nameof(tag.Label));

        return _repo.CreateAsync(tag, ct);
    }

    public Task<List<Tag>> ListByCategoryAsync(long categoryId, CancellationToken ct)
        => _repo.ListByCategoryAsync(categoryId, ct);

    public Task AssignToExpenseAsync(long expenseId, long tagId, CancellationToken ct)
        => _repo.AssignToExpenseAsync(expenseId, tagId, ct);
}
