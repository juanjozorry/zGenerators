using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System.Globalization;
using zPdfGenerator.Html;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Samples.Html
{
    // Implementación de la “aplicación” principal
    public class HtmlSample : ISample
    {
        private readonly ILogger<HtmlSample> _logger;
        private readonly IFluidHtmlTemplatePdfGenerator _generator;

        public HtmlSample(ILogger<HtmlSample> logger, IFluidHtmlTemplatePdfGenerator generator)
        {
            _logger = logger;
            _generator = generator;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting PoC {Time}", DateTimeOffset.Now);

            var report = new CorporateReport
            {
                Subtitle = "Informe del trimestre Q4",
                Date = DateTime.Today,
                ImageBannerUrl = "https://images.unsplash.com/photo-152179...etc",
                ComplementaryImageUrl = "https://images.unsplash.com/photo-153522...etc",
                Metrics = new List<MetricItem>
                {
                    new MetricItem { Title = "Ventas Totales", Value = "1.250.000 €" },
                    new MetricItem { Title = "Clientes Activos", Value = "842" },
                    new MetricItem { Title = "Crecimiento", Value = "12.5%" }
                },
                TableRows = new List<DataRowItem>
                {
                    new DataRowItem { Concept = "Producto A", Value = 234000, Currency = "€", Date = new DateTime(2025, 01, 10) },
                    new DataRowItem { Concept = "Producto B", Value = 98000, Currency = "€", Date = new DateTime(2025, 01, 12) },
                    new DataRowItem { Concept = "Producto C", Value = 12000, Currency = "€", Date = new DateTime(2025, 01, 14) }
                }
            };

            var (pfxBytes, password, _) = Helper.CreateSelfSignedPfx();

            var options = new PdfSignatureOptions(
                pfxPassword: password,
                fieldName: "Signature1",
                appendMode: true,
                visible: false
            );

            Action<FluidHtmlPdfGeneratorBuilder<CorporateReport>> config = b => b
                .UseTemplatePath(Path.Combine(AppContext.BaseDirectory, "Html", "template.html"))
                .UseCulture(new CultureInfo("es-ES"))
                .SetData(report)
                .AddText("Subtitle", i => i.Subtitle)
                .AddDate("ReportDate", i => i.Date)
                .AddCollection("Metrics", i => i.Metrics)
                .AddCollection("TableRows", i => i.TableRows)
                .AddPieChart("chartSvg", i => i.TableRows, r => r.Concept, r => Convert.ToDouble(r.Value), overrideGlobalCultureInfo: new CultureInfo("en-US"),
                    configuration: new PieChartConfig { Title = "Prueba de tarta", Legend = "Leyenda", InsideLabelFormat = "{0:n0}", OutsideLabelFormat = "{1}", PaletteHex = new[] { "#2563EB", "#F59E0B", "#16A34A" } })

                .AddPostDocumentClassifier(PostProcessors.ClassificationEnum.Confidential)
                .AddPostPasswordProtect("MASTER", "USER")
                .AddPostPfxDigitalSignature(pfxBytes, new PdfSignatureOptions(
                                pfxPassword: password,
                                fieldName: "Signature1",
                                appendMode: true,
                                visible: true,
                                pageNumber: 1,
                                x: 36,
                                y: 36,
                                width: 200,
                                height: 60,
                                reason: "Document signed for integrity",
                                location: "My Company",
                                existingPdfPassword: "MASTER", // <- this is required because we password-protected the PDF before
                                cryptoStandard: iText.Signatures.PdfSigner.CryptoStandard.CMS));


            var htmlFileContents = _generator.RenderHtml<CorporateReport>(config);
            var pdfFileContents = _generator.GeneratePdf<CorporateReport>(config);

            await File.WriteAllTextAsync(Path.Combine(AppContext.BaseDirectory, "Html\\SampleHtml.html"), htmlFileContents);
            await File.WriteAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "Html\\SampleHtml.pdf"), pdfFileContents);

            _logger.LogInformation("Finishing PoC");
        }
    }

    public static class Helper
    {
        public static (byte[] pfxBytes, string password, X509Certificate certificate) CreateSelfSignedPfx(string subjectCn = "Sample", int keySize = 2048)
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

    }

    public class CorporateReport
    {
        public string Subtitle { get; set; } = "";
        public DateTime Date { get; set; }

        public List<MetricItem> Metrics { get; set; } = new();
        public List<DataRowItem> TableRows { get; set; } = new();

        public string ImageBannerUrl { get; set; } = "";
        public string ComplementaryImageUrl { get; set; } = "";
    }

    public class MetricItem
    {
        public string Title { get; set; } = "";
        public string Value { get; set; } = "";
    }

    public class DataRowItem
    {
        public string Concept { get; set; } = "";
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public string Currency { get; set; } = "";
    }
}