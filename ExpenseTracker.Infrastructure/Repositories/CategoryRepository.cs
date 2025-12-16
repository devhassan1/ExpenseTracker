using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class CategoryRepository : ICategoryRepository
    {
        private readonly IRepository<Category> _repo;

        public CategoryRepository(IRepository<Category> repo) => _repo = repo;

        public Task<Category?> GetById(long id, CancellationToken ct)
            => _repo.GetById(id, ct);

        public async Task<Category?> GetByLabel(string label, CancellationToken ct)
            => await _repo.Query().AsNoTracking().FirstOrDefaultAsync(c => c.Label == label, ct);

        public async Task<long> Create(Category category, CancellationToken ct)
        {
            await _repo.Add(category, ct);
            // Save will be performed by IUnitOfWork
            return category.Id;
        }

        public async Task<IReadOnlyList<Category>> ListAvailable(long? ownerUserId, CancellationToken ct)
        {
            var q = _repo.Query().AsNoTracking();
            return await q.ToListAsync(ct);
        }
    }

}
