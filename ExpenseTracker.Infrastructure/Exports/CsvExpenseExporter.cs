using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Exports;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Exports
{

    public sealed class CsvExpenseExporter : IExpenseExporter
    {
        public string Format => "csv";

        public Task<byte[]> ExportAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default)
        {
            var sb = new StringBuilder(16 * 1024);
            sb.AppendLine("Date,Amount,Currency,Description,Tags,User");

            foreach (var e in items ?? Enumerable.Empty<ExpenseListItem>())
            {
                ct.ThrowIfCancellationRequested();

                var date = FormatDate(e.TxnDate);
                var amount = e.Amount.ToString("0.##", CultureInfo.InvariantCulture);
                var currency = e.Currency ?? string.Empty;
                var desc = e.Description ?? string.Empty;
                var tags = JoinTags(e);
                var user = !string.IsNullOrWhiteSpace(e.UserName)
                                ? e.UserName!
                                : (e.UserId > 0 ? e.UserId.ToString() : string.Empty);

                sb.AppendLine(string.Join(",",
                    Csv(date), Csv(amount), Csv(currency), Csv(desc), Csv(tags), Csv(user)));
            }

            // Optional: prepend UTF-8 BOM for Excel compatibility
            // var utf8Bom = new byte[] { 0xEF, 0xBB, 0xBF };
            // return utf8Bom.Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

            return Task.FromResult(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        // helpers (you can extract these to a shared static class)
        private static string Csv(string? value)
        {
            var s = value ?? string.Empty;
            var needsQuotes = s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"');
            if (s.Contains('"')) s = s.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{s}\"" : s;
        }

        private static string FormatDate(object? date) => date switch
        {
            null => string.Empty,
            DateOnly d => d.ToString("yyyy-MM-dd"),
            DateTime dt => dt.ToString("yyyy-MM-dd"),
            _ => date.ToString() ?? string.Empty
        };

        private static string JoinTags(ExpenseListItem e)
        {
            if (e.Tags is null) return string.Empty;

            if (e.Tags is IEnumerable<object> objTags)
            {
                var labels = new List<string>();
                foreach (var t in objTags)
                {
                    var labelProp = t?.GetType().GetProperty("Label");
                    var label = labelProp?.GetValue(t) as string;
                    if (!string.IsNullOrWhiteSpace(label)) labels.Add(label);
                }
                return string.Join("; ", labels);
            }
            return string.Empty;
        }
    }
}
