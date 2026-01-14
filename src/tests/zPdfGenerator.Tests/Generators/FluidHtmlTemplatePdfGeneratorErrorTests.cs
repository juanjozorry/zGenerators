using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Generators
{
    public class FluidHtmlTemplatePdfGeneratorErrorTests
    {
        [Fact]
        public void RenderHtml_InvalidTemplate_Throws()
        {
            var templatePath = CreateTemplateFile("<p>{% if %}broken{% endif %}</p>");
            var logger = new NullLogger<FluidHtmlTemplatePdfGenerator>();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, new NoopConverter());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.RenderHtml(templatePath, new Dictionary<string, object?>(), CultureInfo.InvariantCulture));

            Assert.Contains("Error parsing template", ex.Message);
        }

        [Fact]
        public void GeneratePdf_Throws_WhenDuplicatePlaceholderNames()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new NullLogger<FluidHtmlTemplatePdfGenerator>();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, new NoopConverter());

            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.GeneratePdf<TestModel>(builder =>
                {
                    builder
                        .UseTemplatePath(templatePath)
                        .UseCulture(CultureInfo.InvariantCulture)
                        .SetData(new TestModel { Name = "Alice" })
                        .AddText("Name", m => m.Name)
                        .AddText("Name", m => m.Name);
                }));

            Assert.Contains("Duplicate placeholder name", ex.Message);
        }

        private static string CreateTemplateFile(string templateContents)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"Template_{Guid.NewGuid():N}.html");
            File.WriteAllText(tempPath, templateContents, Encoding.UTF8);
            return tempPath;
        }

        private sealed class TestModel
        {
            public string Name { get; set; } = string.Empty;
        }

        private sealed class NoopConverter : IHtmlToPdfConverter
        {
            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
            {
                return Encoding.UTF8.GetBytes(htmlContents ?? string.Empty);
            }
        }

        private sealed class NullLogger<T> : ILogger<T>
        {
            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
            }
        }
    }
}
