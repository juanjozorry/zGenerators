using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using iText.Layout;
using iText.Layout.Element;

using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;


namespace zPdfGenerator.Tests.PostProcessors
{
    public static class TestHelpers
    {
        public static byte[] CreateMinimalPdf(string text = "Hello")
        {
            using var ms = new MemoryStream();
            using var writer = new PdfWriter(ms);
            using var pdf = new PdfDocument(writer);
            using var doc = new Document(pdf);
            doc.Add(new Paragraph(text));
            doc.Close();
            return ms.ToArray();
        }

        public static string ExtractPageText(byte[] pdfBytes, int pageNumber = 1)
        {
            using var ms = new MemoryStream(pdfBytes);
            using var reader = new PdfReader(ms);
            using var pdf = new PdfDocument(reader);
            var page = pdf.GetPage(pageNumber);
            return PdfTextExtractor.GetTextFromPage(page, new SimpleTextExtractionStrategy());
        }

        /// <summary>
        /// Creates a self-signed certificate and returns PFX bytes + password.
        /// BouncyCastle 2.6.2 compatible.
        /// </summary>
        public static (byte[] pfxBytes, string password, X509Certificate certificate) CreateSelfSignedPfx(string subjectCn = "UnitTest", int keySize = 2048)
        {
            // Key pair
            var random = new SecureRandom();
            var keyGen = new RsaKeyPairGenerator();
            keyGen.Init(new KeyGenerationParameters(random, keySize));
            AsymmetricCipherKeyPair keyPair = keyGen.GenerateKeyPair();

            // Certificate
            var certGen = new X509V3CertificateGenerator();
            var serial = BigInteger.ProbablePrime(120, random);
            certGen.SetSerialNumber(serial);

            var subject = new X509Name($"CN={subjectCn}");
            certGen.SetIssuerDN(subject);
            certGen.SetSubjectDN(subject);
            certGen.SetNotBefore(DateTime.UtcNow.AddMinutes(-5));
            certGen.SetNotAfter(DateTime.UtcNow.AddDays(30));
            certGen.SetPublicKey(keyPair.Public);

            certGen.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
            certGen.AddExtension(X509Extensions.KeyUsage, true,
                new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.NonRepudiation));

            var sigFactory = new Asn1SignatureFactory("SHA256WITHRSA", keyPair.Private, random);
            X509Certificate cert = certGen.Generate(sigFactory);

            // Build PFX
            var password = "test-password";
            var store = new Pkcs12StoreBuilder().Build();

            // Friendly alias
            var alias = "signing-key";

            store.SetKeyEntry(
                alias,
                new AsymmetricKeyEntry(keyPair.Private),
                new[] { new X509CertificateEntry(cert) }
            );

            using var ms = new MemoryStream();
            store.Save(ms, password.ToCharArray(), random);

            return (ms.ToArray(), password, cert);
        }

        public static PdfDocumentInfo ReadDocumentInfo(byte[] pdfBytes)
        {
            var ms = new MemoryStream(pdfBytes);
            var reader = new PdfReader(ms);
            var pdf = new PdfDocument(reader);
            // caller should Dispose -> but tests can wrap in using and read needed fields
            return pdf.GetDocumentInfo();
        }
    }
}
