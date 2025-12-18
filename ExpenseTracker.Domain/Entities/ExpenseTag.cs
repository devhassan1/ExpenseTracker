using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Domain.Entities
{

    namespace ExpenseTracker.Domain.Entities
    {
        public sealed class ExpenseTag
        {
            public long ExpenseId { get; set; }
            public Expense Expense { get; set; } = default!;

            public long TagId { get; set; }
            public Tag Tag { get; set; } = default!;
        }
    }
}
