using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using zPdfGenerator.Html;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;

namespace zPdfGenerator.Tests.Charts
{
    public class ChartBuilderExtensionsTests
    {
        [Fact]
        public void AddPieChart_Extension_RendersSvgIntoTemplate()
        {
            var templatePath = CreateTemplateFile("<div>{{ chartSvg | raw }}</div>");
            var logger = new NullLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new CapturingConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var report = new Report
            {
                Items = new List<PieRow>
                {
                    new PieRow("A", 10),
                    new PieRow("B", 20)
                }
            };

            generator.GeneratePdf<Report>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseCulture(CultureInfo.InvariantCulture)
                    .SetData(report)
                    .AddPieChart(
                        name: "chartSvg",
                        map: r => r.Items,
                        label: r => r.Label,
                        value: r => r.Value,
                        configuration: new PieChartConfig { Title = "My Pie" });
            });

            Assert.False(string.IsNullOrWhiteSpace(converter.LastHtml));
            Assert.Contains("<svg", converter.LastHtml);
            Assert.Contains("My Pie", converter.LastHtml);
        }

        private static string CreateTemplateFile(string templateContents)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"Template_{Guid.NewGuid():N}.html");
            File.WriteAllText(tempPath, templateContents, Encoding.UTF8);
            return tempPath;
        }

        private sealed class Report
        {
            public List<PieRow> Items { get; set; } = new();
        }

        private sealed record PieRow(string Label, double Value);

        private sealed class CapturingConverter : IHtmlToPdfConverter
        {
            public string? LastHtml { get; private set; }

            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
            {
                LastHtml = htmlContents;
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
