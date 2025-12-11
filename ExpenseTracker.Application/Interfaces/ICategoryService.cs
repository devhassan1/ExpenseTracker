using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces
{
    public interface ICategoryService
    {
        Task<Category?> GetByIdAsync(long id, CancellationToken ct);
        Task<Category?> GetByLabelAsync(string label, CancellationToken ct);
        Task<long> CreateAsync(Category category, CancellationToken ct);
        Task<IReadOnlyList<Category>> ListAvailableAsync(long? ownerUserId, CancellationToken ct);
    }
}
