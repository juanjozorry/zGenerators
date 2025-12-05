using System.Globalization;
using System.Text;
using Xunit.Abstractions;
using zPdfGenerator.Html;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;

namespace zPdfGenerator.Tests
{
    public class HtmlPdfGeneratorTests
    {
        private readonly ITestOutputHelper _output;

        public HtmlPdfGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void GeneratePdf_RendersFluidTemplateCorrectly()
        {
            // Arrange
            var templatePath = CreateInvoiceTemplateFile();

            var invoice = new Invoice
            {
                Number = "INV-001",
                Date = new DateTime(2025, 3, 12),
                Customer = new Customer { Name = "Juan Pérez" },
                Total = 1234.56m,
                Currency = "EUR",
                DiscountTotal = 50m,
                Lines = new List<InvoiceLine>
                {
                    new InvoiceLine
                    {
                        Description = "Servicio de consultoría",
                        Quantity = 2,
                        Price = 500m
                    },
                    new InvoiceLine
                    {
                        Description = "Licencia software",
                        Quantity = 1,
                        Price = 234.56m
                    }
                }
            };

            var logger = new XunitLogger<FluidHtmlTemplatePdfGenerator>(_output);
            var capturingConverter = new CapturingHtmlToPdfConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, capturingConverter);

            // Act
            var pdfBytes = generator.GeneratePdf<Invoice>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseCulture(new CultureInfo("es-ES"))
                    .SetData(invoice)
                    .AddText("InvoiceNumber", i => i.Number)
                    .AddCultureDate("InvoiceDate", i => i.Date, "dd/MM/yyyy")
                    .AddText("CustomerName", i => i.Customer.Name)
                    .AddCultureNumericAndText(
                        "TotalFormatted",
                        i => new NumericAndTextValue(i.Total, i.Currency))
                    .AddFlag("ShowDiscounts", i => i.DiscountTotal > 0)
                    .AddCollection("Lines", i => i.Lines);
            });

            // Assert over the "PDF" - just check if bytes are correctly generated
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);

            // Assert about rendered HTML
            var renderedHtml = capturingConverter.LastHtml;
            _output.WriteLine("Rendered HTML:");
            _output.WriteLine(renderedHtml);

            Assert.False(string.IsNullOrWhiteSpace(renderedHtml));

            // 1) Check if placeholders are resolved
            Assert.DoesNotContain("{{ InvoiceNumber }}", renderedHtml);
            Assert.DoesNotContain("{{ CustomerName }}", renderedHtml);
            Assert.DoesNotContain("{{ InvoiceDate }}", renderedHtml);
            Assert.DoesNotContain("{{ TotalFormatted }}", renderedHtml);

            Assert.Contains("Invoice INV-001", renderedHtml);
            Assert.Contains("Date: 12/03/2025", renderedHtml);
            Assert.Contains("Customer: Juan Pérez", renderedHtml);
            Assert.Contains("1.234,56 EUR", renderedHtml); // es-ES

            // 2) Check if lines have been rendered
            Assert.Contains("Servicio de consultoría", renderedHtml);
            Assert.Contains("Licencia software", renderedHtml);

            // 3) Check if the conditional sections has been shown
            Assert.Contains("Discounts section visible", renderedHtml);

            // 4) Check if any fluid tag is pending
            Assert.DoesNotContain("{%", renderedHtml);
        }

        #region Helpers

        private string CreateInvoiceTemplateFile()
        {
            var cssPath = Path.Combine(Path.GetTempPath(), "Invoice.css");
            var css = @"
body {
    font-family: Arial, sans-serif;
    font-size: 10pt;
}
h1 {
    color: #2c3e50;
    margin-bottom: 10px;
}
table {
    border-collapse: collapse;
    width: 100%;
    margin-top: 10px;
}
th, td {
    border: 1px solid #333;
    padding: 4px 6px;
}
th {
    background-color: #f0f0f0;
}
.discounts {
    margin-top: 20px;
    padding: 10px;
    border: 1px dashed #888;
    background-color: #fffaf0;
}
";

            File.WriteAllText(cssPath, css, Encoding.UTF8);
            var tempPath = Path.Combine(
                Path.GetTempPath(),
                $"InvoiceTemplate_{Guid.NewGuid():N}.html");

            var template = @"<!DOCTYPE html>
<html>
  <head>
    <meta charset=""utf-8"" />
    <title>Invoice {{ InvoiceNumber }}</title>
    <link rel=""stylesheet"" href=""Invoice.css"" />
  </head>
  <body>
    <h1>Invoice {{ InvoiceNumber }}</h1>
    <p>Date: {{ InvoiceDate }}</p>
    <p>Customer: {{ CustomerName }}</p>
    <p>Total: {{ TotalFormatted }}</p>

    <h2>Lines</h2>
    <table>
      <thead>
        <tr>
          <th>Description</th>
          <th>Qty</th>
          <th>Price</th>
          <th>Subtotal</th>
        </tr>
      </thead>
      <tbody>
        {% for line in Model.Lines %}
          <tr>
            <td>{{ line.Description }}</td>
            <td>{{ line.Quantity }}</td>
            <td>{{ line.Price }}</td>
            <td>{{ line.Subtotal }}</td>
          </tr>
        {% endfor %}
      </tbody>
    </table>

    {% if ShowDiscounts %}
      <div class=""discounts"">
        <h3>Discounts</h3>
        <p>Discounts section visible</p>
      </div>
    {% endif %}
  </body>
</html>";

            File.WriteAllText(tempPath, template, Encoding.UTF8);
            return tempPath;
        }

        #endregion

        #region Test support classes

        private class CapturingHtmlToPdfConverter : IHtmlToPdfConverter
        {
            public string? LastHtml { get; private set; }

            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
            {
                LastHtml = htmlContents;
                // For this test, we just return the HTML bytes as a dummy PDF content
                return Encoding.UTF8.GetBytes(htmlContents ?? string.Empty);
            }
        }

        private class Invoice
        {
            public string Number { get; set; } = string.Empty;
            public DateTime? Date { get; set; }
            public Customer Customer { get; set; } = new Customer();
            public decimal? Total { get; set; }
            public string Currency { get; set; } = "EUR";
            public List<InvoiceLine> Lines { get; set; } = new();
            public decimal DiscountTotal { get; set; }
        }

        private class Customer
        {
            public string Name { get; set; } = string.Empty;
        }

        private class InvoiceLine
        {
            public string Description { get; set; } = string.Empty;
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public decimal Subtotal => Quantity * Price;
        }

        #endregion
    }
}
