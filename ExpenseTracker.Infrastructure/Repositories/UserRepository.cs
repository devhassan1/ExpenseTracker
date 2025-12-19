
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
            var exists = await _db.Users.AnyAsync(x => x.Name == user.Name, ct);
            if (exists) throw new InvalidOperationException("Username already exists");

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user.Id;
        }


        public async Task<Result<long>> RegisterUser(RegisterRequest req, CancellationToken ct)
        {
            // Example duplicate check: by Name (username). Adjust if you use Email, etc.
            var exists = await _db.Users
                .Where(x => x.Name == req.Username)
                .CountAsync(ct )>0;
            if (exists)
                return Result<long>.Fail("Username already exists");

            // TODO: Hash passwords! Never store plain text.
            // You can use ASP.NET Core Identity's PasswordHasher<TUser> or PBKDF2/Argon2.
            var user = new User
            {
                Name = req.Username,
                Email = req.Email,
                RoleId = req.RoleId,
                parent_user_id = req.ParentUserId,
                PasswordHash = req.Password
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return Result<long>.Success(user.Id);
        }

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
    }
}
