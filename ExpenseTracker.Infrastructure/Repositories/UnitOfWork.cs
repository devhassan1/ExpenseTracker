using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace ExpenseTracker.Domain.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly OracleDbContext _db;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(OracleDbContext db) => _db = db;

    public async Task<int> SaveChanges(CancellationToken cancellationToken = default)
        => await _db.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransaction(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            // already in a transaction -> just run
            await operation();
            return;
        }

        try
        {
            _currentTransaction = await _db.Database.BeginTransactionAsync(cancellationToken);
            await operation();
            await _db.SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
        }
        catch
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
            throw;
        }
        finally
        {
            if (_currentTransaction is not null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
    }
}
