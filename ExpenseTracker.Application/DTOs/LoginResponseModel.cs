using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.DTOs
{
    public class LoginResponseModel
    {
        public int UserID { get; set; }
        public string Role { get; set; }
        public bool IsValid { get; set; }
    }
}
