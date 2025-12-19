
using ExpenseTracker.Application.Interfaces.Exports;

public sealed class ExpenseExporterFactory : IExpenseExporterFactory
{
    private readonly IReadOnlyDictionary<string, IExpenseExporter> _map;

    public ExpenseExporterFactory(IEnumerable<IExpenseExporter> exporters)
    {
        var dict = new Dictionary<string, IExpenseExporter>(StringComparer.OrdinalIgnoreCase);
        foreach (var e in exporters)
        {
            dict[e.Format] = e;
            if (e.Format.Equals("xlsx", StringComparison.OrdinalIgnoreCase))
                dict["excel"] = e; // alias support
        }
        _map = dict;
    }

    public IExpenseExporter? Get(string? format)
    {
        var key = (format ?? "csv").Trim().ToLowerInvariant();
        return _map.TryGetValue(key, out var exp) ? exp : null;
    }
}
