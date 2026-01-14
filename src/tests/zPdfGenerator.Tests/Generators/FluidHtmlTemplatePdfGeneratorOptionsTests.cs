using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Generators
{
    public class FluidHtmlTemplatePdfGeneratorOptionsTests
    {
        [Fact]
        public void GeneratePdf_PassesResourcePolicyToConverter()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new CapturingPolicyConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var policy = new HtmlResourceAccessPolicy()
                .AllowSchemes("file");

            var pdf = generator.GeneratePdf<TestModel>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseCulture(CultureInfo.InvariantCulture)
                    .UseResourceAccessPolicy(policy)
                    .SetData(new TestModel { Name = "Alice" })
                    .AddText("Name", m => m.Name);
            });

            Assert.NotNull(pdf);
            Assert.NotEmpty(pdf);
            Assert.Same(policy, converter.LastPolicy);
            Assert.Contains("Alice", converter.LastHtml ?? string.Empty);
        }

        [Fact]
        public void GeneratePdf_TruncatesRenderedHtml_WhenMaxLengthConfigured()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new CapturingPolicyConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var longName = new string('a', 200);

            generator.GeneratePdf<TestModel>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseRenderedHtmlLogMaxLength(20)
                    .SetData(new TestModel { Name = longName })
                    .AddText("Name", m => m.Name);
            });

            Assert.Contains(logger.Messages, m => m.Contains("...(truncated)", StringComparison.Ordinal));
        }

        [Fact]
        public void GeneratePdf_DoesNotLogRenderedHtml_WhenDisabled()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new CapturingPolicyConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            generator.GeneratePdf<TestModel>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseRenderedHtmlLogging(false)
                    .SetData(new TestModel { Name = "NoLog" })
                    .AddText("Name", m => m.Name);
            });

            Assert.DoesNotContain(logger.Messages, m => m.Contains("Template after", StringComparison.Ordinal));
        }

        [Fact]
        public void UseRenderedHtmlLogMaxLength_Throws_WhenNonPositive()
        {
            var builder = new FluidHtmlPdfGeneratorBuilder<TestModel>();

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.UseRenderedHtmlLogMaxLength(0));
        }

        [Fact]
        public void GeneratePdf_Throws_WhenOptionsInvalid()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new CapturingPolicyConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var options = new FluidHtmlPdfGenerationOptions
            {
                RenderedHtmlLogMaxLength = 0
            };

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                generator.GeneratePdf(
                    templatePath,
                    licensePath: null,
                    model: new Dictionary<string, object?> { ["Name"] = "Alice" },
                    culture: CultureInfo.InvariantCulture,
                    options: options,
                    cancellationToken: CancellationToken.None));
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

        private sealed class CapturingPolicyConverter : IHtmlToPdfConverterWithPolicy
        {
            public string? LastHtml { get; private set; }
            public HtmlResourceAccessPolicy? LastPolicy { get; private set; }

            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
            {
                return ConvertHtmlToPDF(htmlContents, basePath, null, cancellationToken);
            }

            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, HtmlResourceAccessPolicy? resourceAccessPolicy, CancellationToken cancellationToken)
            {
                LastHtml = htmlContents;
                LastPolicy = resourceAccessPolicy;
                return Encoding.UTF8.GetBytes(htmlContents ?? string.Empty);
            }
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }

            public List<string> Messages { get; } = new();

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (formatter is null) return;
                Messages.Add(formatter(state, exception));
            }
        }
    }
}
