
//// Infrastructure/Exports/PdfExpenseExporter.cs
//using ExpenseTracker.Application.DTOs;
//using ExpenseTracker.Application.Interfaces.Repositories;
//using QuestPDF.Fluent;
//using QuestPDF.Helpers;
//using QuestPDF.Infrastructure;
//using System.Globalization;

//public sealed class PdfExportService : IExportService
//{
//    public string Format => "pdf";

//    static PdfExportService()
//    {
//        QuestPDF.Settings.License = LicenseType.Community;
//    }

//    public Task<byte[]> ExportExpensesAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default)
//    {
//        var list = (items ?? Enumerable.Empty<ExpenseListItem>()).ToList();
//        var bytes = Document.Create(c =>
//        {
//            c.Page(p =>
//            {
//                p.Size(PageSizes.A4);
//                p.Margin(20);
//                p.DefaultTextStyle(x => x.FontSize(10));
//                p.Header().Text("Expenses Report").SemiBold().FontSize(16);
//                p.Content().Table(t =>
//                {
//                    t.ColumnsDefinition(cols =>
//                    {
//                        cols.RelativeColumn(2); // Date
//                        cols.RelativeColumn(2); // Amount
//                        cols.RelativeColumn(2); // Currency
//                        cols.RelativeColumn(6); // Description
//                        cols.RelativeColumn(4); // Tags
//                        cols.RelativeColumn(3); // User
//                    });
//                    t.Header(h =>
//                    {
//                        h.Cell().Text("Date").Bold();
//                        h.Cell().Text("Amount").Bold();
//                        h.Cell().Text("Currency").Bold();
//                        h.Cell().Text("Description").Bold();
//                        h.Cell().Text("Tags").Bold();
//                        h.Cell().Text("User").Bold();
//                    });
//                    foreach (var e in list)
//                    {
//                        if (ct.IsCancellationRequested) ct.ThrowIfCancellationRequested();
//                        t.Cell().Text(FormatDate(e.TxnDate));
//                        t.Cell().Text(e.Amount.ToString("0.##", CultureInfo.InvariantCulture));
//                        t.Cell().Text(e.Currency ?? string.Empty);
//                        t.Cell().Text(e.Description ?? string.Empty);
//                        t.Cell().Text(JoinTags(e));
//                        t.Cell().Text(!string.IsNullOrWhiteSpace(e.UserName) ? e.UserName! :
//                                      (e.UserId > 0 ? e.UserId.ToString() : string.Empty));
//                    }
//                });
//                p.Footer().AlignRight().Text(x => { x.Span("Page "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
//            });
//        }).GeneratePdf();
//        return Task.FromResult(bytes);
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