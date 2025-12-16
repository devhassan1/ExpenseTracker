using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.Interfaces.Common
{
    public interface IClock { DateTime UtcNow { get; } }
}
