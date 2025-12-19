using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Common.Results;
using ExpenseTracker.Application.UseCases.Reports;

namespace ExpenseTracker.Web.Facades;

public sealed class ReportFacade
{
    private readonly ExportExpensesUseCase _export;

    public ReportFacade(ExportExpensesUseCase export) => _export = export;

    public Task<Result<byte[]>> ExportAsync(ExportRequest req, CancellationToken ct)
        => _export.ExecuteAsync(req, ct);
}
