
// src/ExpenseTracker.Application/Interfaces/Repositories/ITagRepository.cs
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByLabelAsync(string label, CancellationToken ct);
    Task<long> CreateAsync(Tag tag, CancellationToken ct);
    Task<List<Tag>> ListByCategoryAsync(long categoryId, CancellationToken ct);
    Task AssignToExpenseAsync(long expenseId, long tagId, CancellationToken ct);
    Task<List<Tag>> ListAllAsync(CancellationToken ct);
}
