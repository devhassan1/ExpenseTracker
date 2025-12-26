using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.DTOs
{
    public class RegisterRequest
    {
        public string Username { get; set; } = default!;
        public string? Email { get; set; }
        public string Password { get; set; } = default!;
        public long RoleId { get; set; }
        public long ParentUserId { get; set; }

        public bool CreateAdmin { get; set; }
    }

}
