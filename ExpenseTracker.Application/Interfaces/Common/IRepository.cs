using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Common
{
    public interface IRepository<T> where T : class
    {
        Task Add(T entity, CancellationToken ct);
        Task<T?> GetById(long id, CancellationToken ct);
        IQueryable<T> Query();
    }
}
