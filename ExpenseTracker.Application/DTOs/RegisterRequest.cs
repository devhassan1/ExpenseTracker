using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.DTOs
{
    public class RegisterRequest
    {
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string Password { get; set; } = default!;
        public long RoleId { get; set; } = 2;
        public long ParentUserId { get; set; } = 0;
    }

}
