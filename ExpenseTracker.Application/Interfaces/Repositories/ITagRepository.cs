
// src/ExpenseTracker.Application/Interfaces/Repositories/ITagRepository.cs
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces.Repositories;

public interface ITagRepository
{
    Task<Tag?> GetByLabel(string label, CancellationToken ct);
    Task<long> Create(Tag tag, CancellationToken ct);
    Task<List<Tag>> ListByCategory(long categoryId, CancellationToken ct);
    Task AssignToExpense(long expenseId, long tagId, CancellationToken ct);
    Task<List<Tag>> ListAll(CancellationToken ct);
}
