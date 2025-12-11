using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using global::ExpenseTracker.Application.Interfaces;


namespace ExpenseTracker.Infrastructure.Auth
{
    public sealed class SystemClock : IClock
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }

}
