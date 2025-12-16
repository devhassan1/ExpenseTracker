using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Common
{
    /// <summary>
    /// Simple unit of work abstraction. Implementations should coordinate
    /// transactional work and persist changes.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Persist changes to the database.
        /// </summary>
        Task<int> SaveChanges(CancellationToken cancellationToken = default);

        /// <summary>
        /// Execute the provided operation inside a database transaction.
        /// The implementation will commit if the operation completes successfully
        /// or rollback if it throws.
        /// </summary>
        Task ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default);
    }
}
