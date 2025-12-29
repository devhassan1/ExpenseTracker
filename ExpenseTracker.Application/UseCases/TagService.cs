
// ExpenseTracker.Application.UseCases/TagService.cs
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Application.Interfaces.Common;

namespace ExpenseTracker.Application.UseCases
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _repo;
        private readonly IUnitOfWork _uow;
        public TagService(ITagRepository repo, IUnitOfWork uow) { _repo = repo; _uow = uow; }

        public Task<List<Tag>> GetAll(CancellationToken ct) => _repo.ListAll(ct);

        public Task<Tag?> GetByLabel(string label, CancellationToken ct) => _repo.GetByLabel(label, ct);

        public Task<IReadOnlyList<Tag>> SearchAsync(string q, int limit, CancellationToken ct)
            => _repo.SearchAsync(q, limit, ct);

        public async Task<long> Create(Tag tag, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(tag.Label))
                throw new ArgumentException("Label is required.", nameof(tag.Label));
            var id = await _repo.Create(tag, ct);
            await _uow.SaveChanges(ct);
            return id;
        }

        public Task<List<Tag>> ListByCategory(long categoryId, CancellationToken ct) => _repo.ListByCategory(categoryId, ct);

        public Task AssignToExpense(long expenseId, long tagId, CancellationToken ct) => _repo.AssignToExpense(expenseId, tagId, ct);
    }
}
