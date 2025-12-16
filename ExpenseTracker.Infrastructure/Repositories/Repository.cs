using ExpenseTracker.Application.Interfaces.Common;
using ExpenseTracker.Domain.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ExpenseTracker.Infrastructure.Repositories;

    public sealed class Repository<T> :IRepository<T> where T : class
{
    private readonly OracleDbContext _db;
    private readonly DbSet<T> _set;

    public Repository(OracleDbContext db)
    {
        _db = db;
        _set = db.Set<T>();
    }

    public async Task Add(T entity, CancellationToken ct)
    {
        await _set.AddAsync(entity, ct);
    }

    public async Task<T?> GetById(long id, CancellationToken ct)
    {
        return await _set.FindAsync(new object[] { id }, ct);
    }

    public IQueryable<T> Query() => _set.AsQueryable();
}
