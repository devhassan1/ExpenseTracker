using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Common
{
    public interface IUnitOfWork : IDisposable
    {
        Task<int> SaveChanges(CancellationToken cancellationToken = default);
        Task ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default);
    }
}
