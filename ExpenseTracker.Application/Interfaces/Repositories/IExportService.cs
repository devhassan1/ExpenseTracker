using ExpenseTracker.Application.DTOs;

namespace ExpenseTracker.Application.Interfaces.Repositories
{
    public interface IExportService
    {

        Task<byte[]> ExportExpensesAsync(
            IEnumerable<ExpenseListItem> items,
            string format,
            CancellationToken ct = default);


    }
}
