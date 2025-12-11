using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Domain.Entities
{

    public class Category
    {
        public long Id { get; set; }
        public string Label { get; set; } = default!;
    }

}
