using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces.Exports
{
    public interface IExpenseExporter
    {
        string Format { get; } // "csv", "pdf", "xlsx"
        Task<byte[]> ExportAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default);
    }

}
