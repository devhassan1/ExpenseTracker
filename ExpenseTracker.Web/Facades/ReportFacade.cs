
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Application.UseCases.Reports;
using ExpenseTracker.Common.Results;

namespace ExpenseTracker.Web.Facades;

public sealed class ReportFacade
{
    private readonly ExportExpensesUseCase _export;

    public ReportFacade(ExportExpensesUseCase export) => _export = export;

    public Task<Result<byte[]>> ExportAsync(ExportRequest req, CancellationToken ct)
        => _export.ExecuteAsync(req, ct);
}
