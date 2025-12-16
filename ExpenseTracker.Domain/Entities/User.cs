using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Domain.Entities
{
   public class User
    {
        public long Id { get; set; }
        public long RoleId { get; set; }
        public string Name { get; set; } = default!;
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = default!;
        public long? parent_user_id { get; set; }

    }

}
