
// Infrastructure/Exports/DocumentExportService.cs
using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using System.Text;

namespace ExpenseTracker.Infrastructure.Exports
{
    public sealed class DocumentExportService : IExportService
    {
        public string Format => throw new NotImplementedException();

        static DocumentExportService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> ExportExpensesAsync(IEnumerable<ExpenseListItem> items, string format, CancellationToken ct = default)
        {
            var fmt = (format ?? "csv").Trim().ToLowerInvariant();

            return fmt switch
            {
                "csv" => Task.FromResult(ExportCsv(items, ct)),
                "pdf" => Task.FromResult(ExportPdf(items, ct)),
                // add "xlsx" later if needed
                _ => throw new NotSupportedException($"Export format '{format}' is not supported.")
            };
        }

        // ---------- CSV ----------
        private static byte[] ExportCsv(IEnumerable<ExpenseListItem> items, CancellationToken ct)
        {
            var sb = new StringBuilder(16 * 1024);
            sb.AppendLine("Date,Amount,Currency,Description,Tags,User");

            foreach (var e in items ?? Enumerable.Empty<ExpenseListItem>())
            {
                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

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

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Csv(string? value)
        {
            var s = value ?? string.Empty;
            var needsQuotes = s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"');
            if (s.Contains('"')) s = s.Replace("\"", "\"\"");
            return needsQuotes ? $"\"{s}\"" : s;
        }

        // ---------- PDF ----------
        private static byte[] ExportPdf(IEnumerable<ExpenseListItem> items, CancellationToken ct)
        {
            var list = (items ?? Enumerable.Empty<ExpenseListItem>()).ToList();

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(stack =>
                        {
                            stack.Item().Text("Expenses Report").SemiBold().FontSize(16);
                            stack.Item().Text($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
                        });
                    });

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Total Rows: {list.Count}").Bold();

                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Date
                                columns.RelativeColumn(2); // Amount
                                columns.RelativeColumn(2); // Currency
                                columns.RelativeColumn(6); // Description
                                columns.RelativeColumn(4); // Tags
                                columns.RelativeColumn(3); // User
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Date").Bold();
                                header.Cell().Text("Amount").Bold();
                                header.Cell().Text("Currency").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Tags").Bold();
                                header.Cell().Text("User").Bold();
                            });

                            foreach (var e in list)
                            {
                                if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();

                                table.Cell().Text(FormatDate(e.TxnDate));
                                table.Cell().Text(e.Amount.ToString("0.##", CultureInfo.InvariantCulture));
                                table.Cell().Text(e.Currency ?? string.Empty);
                                table.Cell().Text(e.Description ?? string.Empty);
                                table.Cell().Text(JoinTags(e));
                                table.Cell().Text(!string.IsNullOrWhiteSpace(e.UserName)
                                                    ? e.UserName!
                                                    : (e.UserId > 0 ? e.UserId.ToString() : string.Empty));
                            }
                        });
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.Span("Page ");
                        text.CurrentPageNumber();
                        text.Span(" / ");
                        text.TotalPages();
                    });
                });
            }).GeneratePdf();

            return bytes;
        }

        // ---------- helpers ----------
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

            //if (e.Tags is IEnumerable<TagDto> s)
            //    return string.Join("; ", s.Where(x => !string.IsNullOrWhiteSpace(x)));

            var objTags = e.Tags as IEnumerable<object>;
            if (objTags != null)
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