using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;

public interface IReportsFacade
{
    Task<Result<byte[]>> ExportExpensesAsync(ExportRequest req, CancellationToken ct = default);
    // Later: Task<Result<IEnumerable<ExpenseListItem>>> ListAsync(...); etc.
}
