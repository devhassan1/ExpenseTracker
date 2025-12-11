using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::ExpenseTracker.Application.Interfaces;
using global::ExpenseTracker.Domain.Entities;
using global::ExpenseTracker.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class CategoryRepository : ICategoryService
    {
        private readonly OracleDbContext _db;
        public CategoryRepository(OracleDbContext db) => _db = db;

        public async Task<Category?> GetByIdAsync(long id, CancellationToken ct)
            => await _db.Categories.FindAsync(new object?[] { id }, ct);

        public async Task<Category?> GetByLabelAsync( string label, CancellationToken ct)
            => await _db.Categories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Label == label, ct);

        public async Task<long> CreateAsync(Category category, CancellationToken ct)
        {
            await _db.Categories.AddAsync(category, ct);
            await _db.SaveChangesAsync(ct);
            return category.Id;
        }

        public async Task<IReadOnlyList<Category>> ListAvailableAsync( long? ownerUserId, CancellationToken ct)
        {
            var q = _db.Categories.AsQueryable();
            return await q.AsNoTracking().ToListAsync(ct);
        }
    }

}
