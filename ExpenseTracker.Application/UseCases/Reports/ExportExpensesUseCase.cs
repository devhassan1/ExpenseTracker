using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.UseCases.Expenses;
using ExpenseTracker.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Application.UseCases.Reports;

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
            new ExpenseTracker.Application.DTOs.ExpenseFilterRequest(req.From, req.To, req.ForUserId), ct);

        if (!listResult.IsSuccess) return Result<byte[]>.Fail(listResult.Error!);

        var bytes = await _export.ExportExpensesAsync(listResult.Value!, req.Format.ToLowerInvariant(), ct);
        return Result<byte[]>.Success(bytes);
    }
}

