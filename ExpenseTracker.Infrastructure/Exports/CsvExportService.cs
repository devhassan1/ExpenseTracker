using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Domain.Entities;

namespace ExpenseTracker.Domain.Exports;

public sealed class CsvExportService : IExportService
{
    public Task<byte[]> ExportExpensesAsync(IEnumerable<Expense> expenses, string format, CancellationToken ct)
    {
        if (format != "csv") throw new NotSupportedException("Only CSV implemented in this sample.");
        var sb = new StringBuilder();
        sb.AppendLine("Id,UserId,Amount,Description,TxnDate,CreatedAt");
        foreach (var e in expenses)
        {
            sb.AppendLine($"{e.Id},{e.UserId},{e.Money.Amount}" +
                          $"\"{e.Description}\",{e.TxnDate:o},{e.CreatedAt:o}");
        }
        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
    }
}

