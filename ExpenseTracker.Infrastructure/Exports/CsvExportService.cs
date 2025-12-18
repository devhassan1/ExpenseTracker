
//// Infrastructure/Exports/CsvExpenseExporter.cs
//using System.Globalization;
//using System.Text;
//using ExpenseTracker.Application.DTOs;
//using ExpenseTracker.Application.Interfaces.Repositories;

//public sealed class CsvExportService : IExportService
//{
//    public string Format => "csv";

//    public static Task<byte[]> ExportExpensesAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default)
//    {
//        var sb = new StringBuilder(16 * 1024);
//        sb.AppendLine("Date,Amount,Currency,Description,Tags,User");

//        foreach (var e in items ?? Enumerable.Empty<ExpenseListItem>())
//        {
//            if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
//            var date = FormatDate(e.TxnDate);
//            var amount = e.Amount.ToString("0.##", CultureInfo.InvariantCulture);
//            var currency = e.Currency ?? string.Empty;
//            var desc = e.Description ?? string.Empty;
//            var tags = JoinTags(e);
//            var user = !string.IsNullOrWhiteSpace(e.UserName) ? e.UserName! :
//                           (e.UserId > 0 ? e.UserId.ToString() : string.Empty);

//            sb.AppendLine(string.Join(",", Csv(date), Csv(amount), Csv(currency), Csv(desc), Csv(tags), Csv(user)));
//        }
//        return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
//    }

//    private static string Csv(string? v)
//    {
//        var s = v ?? string.Empty;
//        var needs = s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"');
//        if (s.Contains('"')) s = s.Replace("\"", "\"\"");
//        return needs ? $"\"{s}\"" : s;
//    }
//    private static string FormatDate(object? d) => d switch
//    {
//        null => string.Empty,
//        DateOnly x => x.ToString("yyyy-MM-dd"),
//        DateTime x => x.ToString("yyyy-MM-dd"),
//        _ => d.ToString() ?? string.Empty
//    };
//    private static string JoinTags(ExpenseListItem e)
//    {
//        if (e.Tags is null) return string.Empty;
//        if (e.Tags is IEnumerable<TagDto> s) return string.Join("; ", s.Where(x => !string.IsNullOrWhiteSpace(x)));
//        var objs = e.Tags as IEnumerable<object>;
//        if (objs is null) return string.Empty;
//        var labels = new List<string>();
//        foreach (var t in objs)
//        {
//            var label = t?.GetType().GetProperty("Label")?.GetValue(t) as string;
//            if (!string.IsNullOrWhiteSpace(label)) labels.Add(label);
//        }
//        return string.Join("; ", labels);
//    }
//}
