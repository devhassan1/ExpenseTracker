using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Domain.Entities
{
    public enum RoleName { SuperAdmin, Admin, User }

    public class Role
    {
        public long Id { get; set; }
        public string Name { get; set; } = default!;
    }

}
