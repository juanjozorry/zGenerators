using System;
using System.IO;
using System.Threading;
using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Html
{
    public class HtmlToPdfConverterTests
    {
        [Fact]
        public void ConvertHtmlToPDF_Throws_WhenHtmlIsNull()
        {
            var converter = new HtmlToPdfConverter();

            Assert.Throws<NullReferenceException>(() =>
                converter.ConvertHtmlToPDF(null!, Path.GetTempPath(), CancellationToken.None));
        }

        [Fact]
        public void ConvertHtmlToPDF_ReturnsPdfBytes()
        {
            var converter = new HtmlToPdfConverter();
            var html = "<html><body><p>Hello</p></body></html>";

            var bytes = converter.ConvertHtmlToPDF(html, Path.GetTempPath(), CancellationToken.None);

            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }

        [Fact]
        public void ConvertHtmlToPDF_Throws_WhenCancelled()
        {
            var converter = new HtmlToPdfConverter();
            var html = "<html><body><p>Hello</p></body></html>";

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Assert.Throws<OperationCanceledException>(() =>
                converter.ConvertHtmlToPDF(html, Path.GetTempPath(), cts.Token));
        }

        [Fact]
        public void ConvertHtmlToPDF_AllowsPolicy_WhenRestrictionsPresent()
        {
            var converter = new HtmlToPdfConverter();
            var html = "<html><body><p>Hello</p></body></html>";

            var policy = new HtmlResourceAccessPolicy()
                .AllowSchemes("file");

            var bytes = converter.ConvertHtmlToPDF(html, Path.GetTempPath(), policy, CancellationToken.None);

            Assert.NotNull(bytes);
            Assert.NotEmpty(bytes);
        }
    }
}
