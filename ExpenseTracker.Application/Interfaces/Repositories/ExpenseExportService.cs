
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Exports;

public sealed class ExpenseExportService : IExportService
{
    private readonly IReadOnlyDictionary<string, IExpenseExporter> _byFormat;

    public ExpenseExportService(IEnumerable<IExpenseExporter> exporters)
    {
        _byFormat = exporters.ToDictionary(x => x.Format, StringComparer.OrdinalIgnoreCase);
    }

    public Task<byte[]> ExportExpensesAsync(IEnumerable<ExpenseListItem> items, string format, CancellationToken ct = default)
    {
        var key = (format ?? "csv").Trim();
        if (!_byFormat.TryGetValue(key, out var exporter))
            throw new NotSupportedException($"Export format '{format}' is not supported.");

        return exporter.ExportAsync(items ?? Enumerable.Empty<ExpenseListItem>(), ct);
    }
}
