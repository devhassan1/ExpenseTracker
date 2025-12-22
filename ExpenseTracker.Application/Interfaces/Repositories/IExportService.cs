
using ExpenseTracker.Application.DTOs;

public interface IExportService
{
    Task<byte[]> ExportExpensesAsync(IEnumerable<ExpenseListItem> items, string format, CancellationToken ct = default);
}
