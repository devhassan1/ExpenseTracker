
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.Interfaces.Services;

public interface ITagService
{
    Task<List<Tag>> GetAllAsync(CancellationToken ct);
    Task<Tag?> GetByLabelAsync(string label, CancellationToken ct);
    Task<long> CreateAsync(Tag tag, CancellationToken ct);
    Task<List<Tag>> ListByCategoryAsync(long categoryId, CancellationToken ct);
    Task AssignToExpenseAsync(long expenseId, long tagId, CancellationToken ct);
}
