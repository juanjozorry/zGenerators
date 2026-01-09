using iText.Kernel.Pdf;
using iText.Signatures;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Tests.PostProcessors
{
    public class PfxDigitalSignaturePostProcessorTests
    {
        [Fact]
        public void Process_SignsPdf_AndSignatureIsPresent()
        {
            var pdf = TestHelpers.CreateMinimalPdf("Sign me!");
            var (pfxBytes, password, _) = TestHelpers.CreateSelfSignedPfx();

            var options = new PdfSignatureOptions(
                pfxPassword: password,
                fieldName: "Signature1",
                appendMode: true,
                visible: false
            );

            var pp = new PfxDigitalSignaturePostProcessor(pfxBytes, options);

            var signed = pp.Process(pdf, CancellationToken.None);

            using var ms = new MemoryStream(signed);
            using var reader = new PdfReader(ms);
            using var pdfDoc = new PdfDocument(reader);

            var su = new SignatureUtil(pdfDoc);
            var names = su.GetSignatureNames();

            Assert.Single(names);
            Assert.Equal("Signature1", names[0]);
        }

        [Fact]
        public void Process_SignatureCoversWholeDocument()
        {
            var pdf = TestHelpers.CreateMinimalPdf("Sign me!");
            var (pfxBytes, password, _) = TestHelpers.CreateSelfSignedPfx();

            var options = new PdfSignatureOptions(pfxPassword: password, fieldName: "Signature1");
            var pp = new PfxDigitalSignaturePostProcessor(pfxBytes, options);

            var signed = pp.Process(pdf, CancellationToken.None);

            using var ms = new MemoryStream(signed);
            using var reader = new PdfReader(ms);
            using var pdfDoc = new PdfDocument(reader);

            var su = new SignatureUtil(pdfDoc);
            Assert.True(su.SignatureCoversWholeDocument("Signature1"));
        }

        [Fact]
        public void Process_VerifiesSignatureIntegrityAndAuthenticity()
        {
            var pdf = TestHelpers.CreateMinimalPdf("Sign me!");
            var (pfxBytes, password, _) = TestHelpers.CreateSelfSignedPfx();

            var options = new PdfSignatureOptions(pfxPassword: password, fieldName: "Signature1");
            var pp = new PfxDigitalSignaturePostProcessor(pfxBytes, options);

            var signed = pp.Process(pdf, CancellationToken.None);

            using var ms = new MemoryStream(signed);
            using var reader = new PdfReader(ms);
            using var pdfDoc = new PdfDocument(reader);

            var su = new SignatureUtil(pdfDoc);

            // Extrae el PKCS7 y verifica
            PdfPKCS7 pkcs7 = su.ReadSignatureData("Signature1");
            Assert.True(pkcs7.VerifySignatureIntegrityAndAuthenticity());
        }

        [Fact]
        public void LastPostProcessor_IsTrue()
        {
            var (pfxBytes, password, _) = TestHelpers.CreateSelfSignedPfx();
            var pp = new PfxDigitalSignaturePostProcessor(pfxBytes, new PdfSignatureOptions(password));
            Assert.True(pp.LastPostProcessor);
        }
    }
}
