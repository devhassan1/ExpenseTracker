
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Repositories;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Application.UseCases.Reports
{
    public sealed class ExportExpensesUseCase
    {
        private readonly ListExpensesUseCase _list;
        private readonly IExportService _export;

        public ExportExpensesUseCase(ListExpensesUseCase list, IExportService export)
        {
            _list = list;
            _export = export;
        }

        public async Task<Result<byte[]>> ExecuteAsync(ExportRequest req, CancellationToken ct)
        {
            var listResult = await _list.ExecuteAsync(
                new ExpenseFilterRequest(req.From, req.To, req.ForUserId), ct);

            if (!listResult.IsSuccess)
                return Result<byte[]>.Fail(listResult.Error!);

            var items = (IEnumerable<ExpenseListItem>)listResult.Value!;

            try
            {
                var fmt = req.Format?.ToLowerInvariant() ?? "csv";
                // Optional: normalize 'excel' to 'xlsx' here if you plan to support Excel later
                var normalized = fmt == "excel" ? "xlsx" : fmt;

                var bytes = await _export.ExportExpensesAsync(items, normalized, ct);
                return Result<byte[]>.Success(bytes);
            }
            catch (NotSupportedException ex)
            {
                // Turn it into a clean failure (400) instead of 500
                return Result<byte[]>.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                // Catch-all to prevent 500 leaks; log ex as needed
                return Result<byte[]>.Fail("Unexpected error during export: " + ex.Message);
            }
        }
    }
}
