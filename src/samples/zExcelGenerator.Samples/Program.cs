using System;
using System.Collections.Generic;
using System.IO;
using ClosedXML.Excel;
using Microsoft.Extensions.Logging.Abstractions;
using zExcelGenerator;

namespace zExcelGenerator.Samples;

internal static class Program
{
    private static void Main()
    {
        var templatePath = Path.Combine(Environment.CurrentDirectory, "InvoiceTemplate.xlsx");
        var outputPath = Path.Combine(Environment.CurrentDirectory, "InvoiceOutput.xlsx");

        CreateTemplate(templatePath);

        var invoice = new Invoice
        {
            CustomerName = "Acme Corp",
            Date = new DateTime(2024, 10, 21),
            Lines = new List<InvoiceLine>
            {
                new InvoiceLine { Sku = "A-01", Qty = 2, Price = 10.5m },
                new InvoiceLine { Sku = "B-02", Qty = 1, Price = 5m }
            }
        };

        var generator = new ExcelGenerator(new NullLogger<ExcelGenerator>());
        var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
            .UseTemplatePath(templatePath)
            .SetData(invoice)
            .ForWorksheet("Cover", ws => ws
                .NamedRange("CustomerName", x => x.CustomerName)
                .NamedRange("InvoiceDate", x => x.Date, format: "dd/MM/yyyy"))
            .ForWorksheet("Lines", ws => ws
                .NamedRangeTable("LinesHeader", x => x.Lines, table => table
                    .Column("Sku", l => l.Sku, 1)
                    .Column("Qty", l => l.Qty, 2, format: "0")
                    .Column("Price", l => l.Price, 3, format: "#,##0.00"),
                    headerRowIsNamedRange: true,
                    writeHeaders: false,
                    insertRows: true))
            .AddWorksheet("Summary", invoice.Lines, ws => ws
                .Column("Sku", l => l.Sku, 1)
                .Column("Qty", l => l.Qty, 2, format: "0")
                .Column("Price", l => l.Price, 3, format: "#,##0.00"))
        );

        File.WriteAllBytes(outputPath, bytes);
    }

    private static void CreateTemplate(string templatePath)
    {
        using var workbook = new XLWorkbook();

        var cover = workbook.AddWorksheet("Cover");
        cover.Cell("A1").Value = "Invoice";
        cover.Cell("A2").Value = "Customer:";
        cover.Cell("A3").Value = "Date:";
        cover.Cell("B2").Value = "CustomerName";
        cover.Cell("B3").Value = "InvoiceDate";
        cover.DefinedNames.Add("CustomerName", cover.Range("B2"));
        cover.DefinedNames.Add("InvoiceDate", cover.Range("B3"));

        var lines = workbook.AddWorksheet("Lines");
        lines.Cell("A1").Value = "Sku";
        lines.Cell("B1").Value = "Qty";
        lines.Cell("C1").Value = "Price";
        lines.DefinedNames.Add("LinesHeader", lines.Range("A1"));

        workbook.SaveAs(templatePath);
    }

    private sealed class Invoice
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public List<InvoiceLine> Lines { get; set; } = new();
    }

    private sealed class InvoiceLine
    {
        public string Sku { get; set; } = string.Empty;
        public int Qty { get; set; }
        public decimal Price { get; set; }
    }
}
