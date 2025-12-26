
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly OracleDbContext _db;

        public UserRepository(OracleDbContext db)
        {
            _db = db;
        }

        public async Task<long> Create(User user, CancellationToken ct)
        {
            var exists = await _db.Users.Where(x => x.Name == user.Name)
    .CountAsync(ct) > 0;
            ;
            if (exists) throw new InvalidOperationException("Username already exists");

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user.Id;
        }


        //public async Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct)
        //{
        //    // Example duplicate check: by Name (username). Adjust if you use Email, etc.
        //    var exists = await _db.Users
        //        .Where(x => x.Name == req.Username)
        //        .CountAsync(ct) > 0;
        //    if (exists)
        //        return Result<long>.Fail("Username already exists");

        //    var user = new User
        //    {
        //        Name = req.Username,
        //        Email = req.Email,
        //        parent_user_id = req.ParentUserId,
        //        RoleId = req.RoleId,
        //        PasswordHash = req.Password
        //    };

        //    _db.Users.Add(user);
        //    await _db.SaveChangesAsync(ct);
        //    return Result<long>.Success(user.Id);
        //}

        public async Task<User?> GetByName(string name)
            => await _db.Users.Where(u => u.Name == name).FirstOrDefaultAsync();

        public async Task<string> GetRoleByID(long id)
        {
            return await _db.Roles.Where(r => r.Id == id).Select(x => x.Name).FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyList<User>> ListByParent(long? parentUserId, CancellationToken ct)
        {
            var query = _db.Users.AsQueryable();
            if (parentUserId is not null)
                query = query.Where(u => u.parent_user_id == parentUserId);

            return await query
                .OrderBy(u => u.Name)
                .ToListAsync(ct);
        }


        public async Task<IEnumerable<User>> ListByParentPaged(
                  long? parentUserId,
                  int page,
                  int pageSize,
                  string? search,
                  CancellationToken ct = default)
        {
            var q = _db.Users.AsNoTracking();

            if (parentUserId.HasValue)
                q = q.Where(u => u.parent_user_id == parentUserId.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();

                q = q.Where(u =>
                    (u.Name != null && u.Name.Contains(term)) ||
                    (u.Email != null && u.Email.Contains(term)));
            }

            // Default sort: Name asc (stable)
            q = q.OrderBy(u => u.Name);

            var safePage = Math.Max(page, 1);
            var safeSize = Math.Clamp(pageSize, 1, 200);

            return await q
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .ToListAsync(ct);
        }
    }
}