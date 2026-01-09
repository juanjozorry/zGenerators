using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using System.Globalization;
using Xunit.Abstractions;
using zPdfGenerator.Forms;
using zPdfGenerator.Forms.FormPlaceHolders;

namespace zPdfGenerator.Tests.Generators
{
    public class FormPdfGeneratorTests
    {
        private readonly ITestOutputHelper _output;

        public FormPdfGeneratorTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private FormPdfGenerator CreateGenerator()
        {
            var logger = new XunitLogger<FormPdfGenerator>(_output);
            return new FormPdfGenerator(logger);
        }

        #region Helpers

        /// <summary>
        /// Creates a temporary PDF form template with a few fields for testing.
        /// Fields: Name (text), BirthDate (text), Balance (text), ToRemove (text).
        /// </summary>
        private string CreateTemplatePdfWithFormFields()
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"FormTemplate_{Guid.NewGuid():N}.pdf");

            using (var writer = new PdfWriter(tempPath))
            using (var pdf = new PdfDocument(writer))
            {
                var document = new iText.Layout.Document(pdf);

                // Add a simple page and some text - form fields will be created with AcroForm.
                document.Add(new iText.Layout.Element.Paragraph("Test PDF Form"));

                var form = PdfAcroForm.GetAcroForm(pdf, true);

                PdfTextFormField CreateTextField(PdfDocument pdf, PdfAcroForm form, string name,
                    float x, float y, float width = 200, float height = 20)
                {
                    var rect = new iText.Kernel.Geom.Rectangle(x, y, width, height);

                    var textField = new TextFormFieldBuilder(pdf, name)
                        .SetWidgetRectangle(rect)
                        .CreateText();

                    form.AddField(textField);
                    return textField;
                }

                // Coordinates: simple layout near bottom of page
                CreateTextField(pdf, form, "Name", 50, 750);
                CreateTextField(pdf, form, "BirthDate", 50, 720);
                CreateTextField(pdf, form, "Balance", 50, 690);
                CreateTextField(pdf, form, "ToRemove", 50, 660);

                document.Close();
            }

            return tempPath;
        }

        /// <summary>
        /// Opens a PDF from bytes and returns its AcroForm fields.
        /// </summary>
        private IDictionary<string, PdfFormField> GetPdfFieldsFromBytes(byte[] pdfBytes)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var pdf = new PdfDocument(new PdfReader(ms));
            var form = PdfAcroForm.GetAcroForm(pdf, false);
            return form.GetAllFormFields();
        }

        #endregion

        #region Tests

        [Fact]
        public void GeneratePdf_Throws_WhenConfigureIsNull()
        {
            // Arrange
            var generator = CreateGenerator();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                generator.GeneratePdf<object>(null!));
        }

        [Fact]
        public void GeneratePdf_Throws_WhenTemplatePathNotConfigured()
        {
            // Arrange
            var generator = CreateGenerator();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.GeneratePdf<Customer>(builder =>
                {
                    // No template
                    builder
                        .SetData(new Customer())
                        .AddText("Name", c => c.Name);
                }));

            Assert.Contains("TemplatePath must be configured", ex.Message);
        }

        [Fact]
        public void GeneratePdf_Throws_WhenDataItemNotSet()
        {
            // Arrange
            var generator = CreateGenerator();
            var templatePath = CreateTemplatePdfWithFormFields();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.GeneratePdf<Customer>(builder =>
                {
                    builder
                        .UseTemplatePath(templatePath)
                        // .SetData(...) missing on purpose
                        .AddText("Name", c => c.Name);
                }));

            Assert.Contains("DataItem must be set up", ex.Message);
        }

        [Fact]
        public void GeneratePdf_Populates_Fields_Correctly()
        {
            // Arrange
            var generator = CreateGenerator();
            var templatePath = CreateTemplatePdfWithFormFields();

            var customer = new Customer
            {
                Name = "John Doe",
                BirthDate = new DateTime(1985, 3, 12),
                Balance = 1234.56m,
                Total = 9876.54m,
                Currency = "EUR"
            };

            // Act
            var pdfBytes = generator.GeneratePdf<Customer>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseCulture(new CultureInfo("es-ES"))
                    .SetData(customer)
                    .AddText("Name", c => c.Name)
                    .AddDate("BirthDate", c => c.BirthDate, "dd/MM/yyyy")
                    .AddNumeric("Balance", c => c.Balance, "N2")
                    .AddNumericAndText("TotalWithCurrency",
                        c => new NumericAndTextValue(c.Total, c.Currency))
                    .AddFormElementsToRemove("ToRemove");
            });

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.NotEmpty(pdfBytes);

            var fields = GetPdfFieldsFromBytes(pdfBytes);

            // Name
            Assert.True(fields.ContainsKey("Name"));
            Assert.Equal("John Doe", fields["Name"].GetValueAsString());

            // BirthDate in dd/MM/yyyy, es-ES
            Assert.True(fields.ContainsKey("BirthDate"));
            Assert.Equal("12/03/1985", fields["BirthDate"].GetValueAsString());

            // Balance formatted as es-ES "1.234,56"
            Assert.True(fields.ContainsKey("Balance"));
            Assert.Equal("1.234,56", fields["Balance"].GetValueAsString());

            // ToRemove should no longer exist
            Assert.False(fields.ContainsKey("ToRemove"));

            // TotalWithCurrency uses NumericAndTextPlaceHolder, but note:
            // In this test the template has no field called "TotalWithCurrency",
            // so the generator should log a warning and skip it. We just assert it doesn't blow up.
        }

        [Fact]
        public void GeneratePdf_Cancels_WhenTokenIsCancelled()
        {
            // Arrange
            var generator = CreateGenerator();
            var templatePath = CreateTemplatePdfWithFormFields();
            var customer = new Customer { Name = "Cancelled" };

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // cancel before calling

            // Act & Assert
            Assert.Throws<OperationCanceledException>(() =>
                generator.GeneratePdf<Customer>(builder =>
                {
                    builder
                        .UseTemplatePath(templatePath)
                        .SetData(customer)
                        .AddText("Name", c => c.Name);
                }, cts.Token));
        }

        #endregion

        #region Test support classes

        private class Customer
        {
            public string Name { get; set; } = string.Empty;
            public DateTime? BirthDate { get; set; }
            public decimal? Balance { get; set; }
            public decimal? Total { get; set; }
            public string Currency { get; set; } = string.Empty;
        }

        #endregion
    }
}
