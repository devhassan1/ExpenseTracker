using ExpenseTracker.Application.DTOs;
using ExpenseTracker.Application.Interfaces.Exports;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Infrastructure.Exports
{

    public sealed class PdfExpenseExporter : IExpenseExporter
    {
        public string Format => "pdf";

        static PdfExpenseExporter()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public Task<byte[]> ExportAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default)
        {
            var list = (items ?? Enumerable.Empty<ExpenseListItem>()).ToList();

            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(QuestPDF.Helpers.PageSizes.A4);
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
                                ct.ThrowIfCancellationRequested();

                                table.Cell().Text(FormatDate(e.TxnDate));
                                table.Cell().Text(e.Amount.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
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

            return Task.FromResult(bytes);
        }

        // reuse helpers (same as CSV exporter)
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
