using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface ICategoryRepository
    {
        Task<Category?> GetById(long id, CancellationToken ct);
        Task<Category?> GetByLabel(string label, CancellationToken ct);
        Task<long> Create(Category category, CancellationToken ct);
        Task<IReadOnlyList<Category>> ListAvailable(long? ownerUserId, CancellationToken ct);
    }
}
