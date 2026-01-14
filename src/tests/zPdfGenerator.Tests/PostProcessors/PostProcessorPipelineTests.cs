using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using zPdfGenerator.Html;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Tests.PostProcessors
{
    public class PostProcessorPipelineTests
    {
        [Fact]
        public void GeneratePdf_Throws_WhenMultipleLastPostProcessorsProvided()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new StubHtmlToPdfConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var ex = Assert.Throws<InvalidOperationException>(() =>
                generator.GeneratePdf<TestModel>(builder =>
                {
                    builder
                        .UseTemplatePath(templatePath)
                        .UseCulture(CultureInfo.InvariantCulture)
                        .SetData(new TestModel { Name = "Alice" })
                        .AddText("Name", m => m.Name)
                        .AddPostProcessor(new MarkerPostProcessor('A', isLast: true))
                        .AddPostProcessor(new MarkerPostProcessor('B', isLast: true));
                }));

            Assert.Contains("Only one LastPostProcessor", ex.Message);
        }

        [Fact]
        public void GeneratePdf_RunsLastPostProcessorAfterNormalOnes()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new StubHtmlToPdfConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            var result = generator.GeneratePdf<TestModel>(builder =>
            {
                builder
                    .UseTemplatePath(templatePath)
                    .UseCulture(CultureInfo.InvariantCulture)
                    .SetData(new TestModel { Name = "Alice" })
                    .AddText("Name", m => m.Name)
                    .AddPostProcessor(new MarkerPostProcessor('N', isLast: false))
                    .AddPostProcessor(new MarkerPostProcessor('L', isLast: true));
            });

            Assert.Equal(new byte[] { 1, (byte)'N', (byte)'L' }, result);
        }

        [Fact]
        public void GeneratePdf_Throws_WhenPostProcessorCancels()
        {
            var templatePath = CreateTemplateFile("<p>{{ Name }}</p>");
            var logger = new CapturingLogger<FluidHtmlTemplatePdfGenerator>();
            var converter = new StubHtmlToPdfConverter();
            var generator = new FluidHtmlTemplatePdfGenerator(logger, converter);

            using var cts = new CancellationTokenSource();

            Assert.Throws<OperationCanceledException>(() =>
                generator.GeneratePdf<TestModel>(builder =>
                {
                    builder
                        .UseTemplatePath(templatePath)
                        .UseCulture(CultureInfo.InvariantCulture)
                        .SetData(new TestModel { Name = "Alice" })
                        .AddText("Name", m => m.Name)
                        .AddPostProcessor(new CancelingPostProcessor(cts));
                }, cts.Token));
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

        private sealed class StubHtmlToPdfConverter : IHtmlToPdfConverter
        {
            public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
            {
                return new byte[] { 1 };
            }
        }

        private sealed class MarkerPostProcessor : IPostProcessor
        {
            private readonly byte _marker;

            public MarkerPostProcessor(char marker, bool isLast)
            {
                _marker = (byte)marker;
                LastPostProcessor = isLast;
            }

            public bool LastPostProcessor { get; }

            public byte[] Process(byte[] pdfData, CancellationToken cancellationToken)
            {
                var result = new byte[pdfData.Length + 1];
                Buffer.BlockCopy(pdfData, 0, result, 0, pdfData.Length);
                result[^1] = _marker;
                return result;
            }
        }

        private sealed class CancelingPostProcessor : IPostProcessor
        {
            private readonly CancellationTokenSource _cts;

            public CancelingPostProcessor(CancellationTokenSource cts)
            {
                _cts = cts;
            }

            public bool LastPostProcessor => false;

            public byte[] Process(byte[] pdfData, CancellationToken cancellationToken)
            {
                _cts.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
                return pdfData;
            }
        }

        private sealed class CapturingLogger<T> : ILogger<T>
        {
            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();
                public void Dispose() { }
            }

            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
            }
        }
    }
}
