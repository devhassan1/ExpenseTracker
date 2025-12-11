using ExpenseTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(long id, CancellationToken ct);
    }
}
