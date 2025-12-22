using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExpenseTracker.Application.DTOs;
using global::ExpenseTracker.Application.Interfaces.Exports;
// Infrastructure/Exports/XlsxExpenseExporter.cs
using System.Globalization;

namespace ExpenseTracker.Infrastructure.Exports
{
        public sealed class XlsxExpenseExporter : IExpenseExporter
        {
            public string Format => "xlsx";

            public Task<byte[]> ExportAsync(IEnumerable<ExpenseListItem> items, CancellationToken ct = default)
            {
                // Build XLSX in memory
                using var stream = new MemoryStream();
                using (var doc = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook, autoSave: true))
                {
                    var wbPart = doc.AddWorkbookPart();
                    wbPart.Workbook = new Workbook();

                    var wsPart = wbPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    wsPart.Worksheet = new Worksheet(sheetData);

                    // Create Sheets collection and add a single Sheet
                    var sheets = doc.WorkbookPart!.Workbook.AppendChild(new Sheets());
                    var sheet = new Sheet
                    {
                        Id = doc.WorkbookPart.GetIdOfPart(wsPart),
                        SheetId = 1,
                        Name = "Expenses"
                    };
                    sheets.Append(sheet);

                    // Header row
                    var header = new Row();
                    header.Append(
                        NewTextCell("Date"),
                        NewTextCell("Amount"),
                        NewTextCell("Currency"),
                        NewTextCell("Description"),
                        NewTextCell("Tags"),
                        NewTextCell("User")
                    );
                    sheetData.Append(header);

                    // Data rows
                    foreach (var e in items ?? Enumerable.Empty<ExpenseListItem>())
                    {
                        ct.ThrowIfCancellationRequested();

                        var row = new Row();

                        // Date: store as text for simplicity (could be numeric with date style)
                        row.Append(NewTextCell(FormatDate(e.TxnDate)));

                        // Amount: store as number
                        row.Append(NewNumberCell(e.Amount.ToString("0.##", CultureInfo.InvariantCulture)));

                        // Currency, Description, Tags, User: text
                        row.Append(NewTextCell(e.Currency ?? string.Empty));
                        row.Append(NewTextCell(e.Description ?? string.Empty));
                        row.Append(NewTextCell(JoinTags(e)));
                        row.Append(NewTextCell(!string.IsNullOrWhiteSpace(e.UserName)
                            ? e.UserName!
                            : (e.UserId > 0 ? e.UserId.ToString() : string.Empty)));

                        sheetData.Append(row);
                    }

                    // Minimal workbook parts
                    wbPart.Workbook.Save();
                }

                return Task.FromResult(stream.ToArray());
            }

            // ---- helpers (kept local; you can move to a shared static helper if preferred) ----

            private static Cell NewTextCell(string? value) =>
                new Cell
                {
                    DataType = CellValues.String,
                    CellValue = new CellValue(value ?? string.Empty)
                };

            private static Cell NewNumberCell(string number) =>
                new Cell
                {
                    DataType = CellValues.Number,
                    CellValue = new CellValue(number)
                };

            private static string FormatDate(object? date) => date switch
            {
                null => string.Empty,
                DateOnly d => d.ToString("yyyy-MM-dd"),
                DateTime dt => dt.ToString("yyyy-MM-dd"),
                _ => date?.ToString() ?? string.Empty
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
