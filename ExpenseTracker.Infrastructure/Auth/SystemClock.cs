using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Application.Interfaces.Common;
using global::ExpenseTracker.Application.Interfaces;


namespace ExpenseTracker.Domain.Auth
{
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

}
