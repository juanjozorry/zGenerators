using iText.Kernel.Pdf;
using zPdfGenerator.PostProcessors;
using zPdfGenerator.Tests.PostProcessors;

namespace zPdfGenerator.Tests.PostProcessors
{
    public class DocumentClassifierPostProcessorTests
    {
        [Fact]
        public void Process_WritesStandardAndCustomMetadata()
        {
            var pdf = TestHelpers.CreateMinimalPdf();

            var pp = new DocumentClassifierPostProcessor(
                classification: ClassificationEnum.Confidential,
                additionalValues: new Dictionary<string, string> { ["Foo"] = "Bar" }
            );

            var result = pp.Process(pdf, CancellationToken.None);

            using var ms = new MemoryStream(result);
            using var reader = new PdfReader(ms);
            using var pdfDoc = new PdfDocument(reader);

            var info = pdfDoc.GetDocumentInfo();

            Assert.Equal("Confidential", info.GetMoreInfo("Classification"));
            Assert.False(string.IsNullOrWhiteSpace(info.GetMoreInfo("SI_DATA")));

            Assert.Contains("Classification:", info.GetSubject() ?? "");

            Assert.Equal("Bar", info.GetMoreInfo("Foo"));
        }

        [Fact]
        public void Process_DoesNotAllowReservedKeysToBeOverridden()
        {
            var pdf = TestHelpers.CreateMinimalPdf();

            var pp = new DocumentClassifierPostProcessor(
                classification: ClassificationEnum.Internal,
                additionalValues: new Dictionary<string, string>
                {
                    ["Classification"] = "Public"
                }
            );

            Assert.Throws<InvalidOperationException>(() =>
                pp.Process(pdf, CancellationToken.None));
        }

        [Fact]
        public void LastPostProcessor_IsFalse()
        {
            var pp = new DocumentClassifierPostProcessor(ClassificationEnum.Public);
            Assert.False(pp.LastPostProcessor);
        }
    }
}