
// ExpenseTracker.Application.UseCases/ITagService.cs
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Application.UseCases
{
    public interface ITagService
    {
        Task<List<Tag>> GetAll(CancellationToken ct);
        Task<Tag?> GetByLabel(string label, CancellationToken ct);
        Task<IReadOnlyList<Tag>> SearchAsync(string q, int limit, CancellationToken ct);
        Task<long> Create(Tag tag, CancellationToken ct);
        Task<List<Tag>> ListByCategory(long categoryId, CancellationToken ct);
        Task AssignToExpense(long expenseId, long tagId, CancellationToken ct);
    }
}
