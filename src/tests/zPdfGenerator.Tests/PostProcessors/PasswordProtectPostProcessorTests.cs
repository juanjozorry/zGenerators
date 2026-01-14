using iText.Kernel.Exceptions;
using iText.Kernel.Pdf;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Tests.PostProcessors
{
    public class PasswordProtectPostProcessorTests
    {
        [Fact]
        public void Process_Throws_WhenPasswordsMissing()
        {
            var pdf = TestHelpers.CreateMinimalPdf("Secret");

            var ex1 = Assert.Throws<ArgumentNullException>(() =>
                new PasswordProtectPostProcessor(masterPassword: "", userPassword: "user").Process(pdf, CancellationToken.None));
            Assert.Contains("MasterPassword", ex1.Message);

            var ex2 = Assert.Throws<ArgumentNullException>(() =>
                new PasswordProtectPostProcessor(masterPassword: "master", userPassword: "").Process(pdf, CancellationToken.None));
            Assert.Contains("UserPassword", ex2.Message);
        }

        [Fact]
        public void Process_EncryptsPdf_WithUserAndMasterPassword()
        {
            var pdf = TestHelpers.CreateMinimalPdf("Secret");

            var pp = new PasswordProtectPostProcessor(
                masterPassword: "owner-secret",
                userPassword: "user-secret"
            );

            var protectedPdf = pp.Process(pdf, CancellationToken.None);

            // Test if the PDF can be opened without password
            Assert.ThrowsAny<PdfException>(() =>
            {
                using var ms = new MemoryStream(protectedPdf);
                using var reader = new PdfReader(ms); // sin password
                using var pdfDoc = new PdfDocument(reader);
            });

            // Test if the PDF can be opened with the user password
            using (var ms = new MemoryStream(protectedPdf))
            {
                var reader = new PdfReader(
                    ms,
                    new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes("user-secret"))
                );

                using var pdfDoc = new PdfDocument(reader);
                Assert.Equal(1, pdfDoc.GetNumberOfPages());
            }

            // Test if the PDF can be opened with the master password
            using (var ms = new MemoryStream(protectedPdf))
            {
                var reader = new PdfReader(
                    ms,
                    new ReaderProperties().SetPassword(System.Text.Encoding.UTF8.GetBytes("owner-secret"))
                );

                using var pdfDoc = new PdfDocument(reader);
                Assert.Equal(1, pdfDoc.GetNumberOfPages());
            }
        }
    }
}
