using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Provides functionality for applying digital signatures to a PDF using a PFX (Personal Information Exchange)
    /// certificate.
    /// </summary>
    public sealed class PfxDigitalSignaturePostProcessor : IPostProcessor
    {
        private readonly byte[] _pfxBytes;
        private readonly PdfSignatureOptions _options;

        /// <summary>
        /// Initializes a new instance of the PfxDigitalSignaturePostProcessor class using the specified PFX certificate
        /// data and signature options.
        /// </summary>
        /// <param name="pfxBytes">A byte array containing the PFX (PKCS #12) certificate data used for digital signing. Must not be null or
        /// empty.</param>
        /// <param name="options">The options that configure how the PDF signature is applied. Must not be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if pfxBytes or options is null.</exception>
        /// <exception cref="ArgumentException">Thrown if pfxBytes is an empty array.</exception>
        public PfxDigitalSignaturePostProcessor(byte[] pfxBytes, PdfSignatureOptions options)
        {
            _pfxBytes = pfxBytes ?? throw new ArgumentNullException(nameof(pfxBytes));
            if (_pfxBytes.Length == 0) throw new ArgumentException("PFX bytes are empty.", nameof(pfxBytes));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        /// <summary>
        /// Gets a value indicating whether this instance represents the last post-processor in the processing sequence.
        /// </summary>
        public bool LastPostProcessor => true;
        
        /// <summary>
        /// Digitally signs a PDF document using the configured certificate and signature options.
        /// </summary>
        /// <remarks>The method applies a detached digital signature to the input PDF using the specified
        /// certificate and options. If visible signature options are configured, the signature appearance will be added
        /// to the specified location in the document. The operation may be computationally intensive due to certificate
        /// loading and cryptographic processing.</remarks>
        /// <param name="pdfData">The PDF document to sign, provided as a byte array. Must not be null or empty.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the signing operation.</param>
        /// <returns>A byte array containing the signed PDF document.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the pdfData parameter is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the pdfData parameter is empty.</exception>
        public byte[] Process(byte[] pdfData, CancellationToken cancellationToken)
        {
            if (pdfData is null) throw new ArgumentNullException(nameof(pdfData));
            if (pdfData.Length == 0) throw new ArgumentException("PDF data is empty.", nameof(pdfData));

            // Load certs/keys (can be a bit expensive)
            var (privateKey, chain) = LoadFromPfx(_pfxBytes, _options.PfxPassword);
            if (privateKey is null) throw new ArgumentException("PDF data is empty.", nameof(pdfData));
            cancellationToken.ThrowIfCancellationRequested();

            using var input = new MemoryStream(pdfData);
            using var output = new MemoryStream();

            PdfReader reader;

            if (!string.IsNullOrEmpty(_options.ExistingPdfPassword))
            {
                var rp = new ReaderProperties()
                    .SetPassword(Encoding.UTF8.GetBytes(_options.ExistingPdfPassword));

                reader = new PdfReader(input, rp);
            }
            else
            {
                reader = new PdfReader(input);
            }

            var stampingProps = new StampingProperties();
            if (_options.AppendMode)
                stampingProps.UseAppendMode();

            var signerProperties = new SignerProperties()
                .SetFieldName(_options.FieldName ?? string.Empty)
                .SetReason(_options.Reason ?? string.Empty)
                .SetLocation(_options.Location ?? string.Empty);

            if (_options.Visible)
            {
                signerProperties
                    .SetPageNumber(_options.PageNumber)
                    .SetPageRect(new Rectangle(_options.X, _options.Y, _options.Width, _options.Height));
            }

            var signer = new PdfSigner(reader, output, stampingProps);
            signer.SetSignerProperties(signerProperties);

            IX509Certificate[] certificateWrappers = new IX509Certificate[chain.Length];
            for (int i = 0; i < chain.Length; i++)
                certificateWrappers[i] = new X509CertificateBC(chain[i]);

            var pks = new PrivateKeySignature(
                new PrivateKeyBC((ICipherParameters)privateKey),
                _options.DigestAlgorithm);
            
            cancellationToken.ThrowIfCancellationRequested();

            // Detached signature. CRL/OCSP/TSA = null for MVP.
            signer.SignDetached(
                pks,
                certificateWrappers,
                null,  // CRL
                null,  // OCSP
                null,  // TSA
                0,
                _options.CryptoStandard
            );
            return output.ToArray();
        }

        private static (AsymmetricKeyParameter privateKey, X509Certificate[] chain) LoadFromPfx(byte[] pfxBytes, string password)
        {
            using var ms = new MemoryStream(pfxBytes);
            var store = new Pkcs12StoreBuilder().Build();
            store.Load(ms, (password ?? string.Empty).ToCharArray());

            var alias = store.Aliases.Cast<string>().FirstOrDefault(store.IsKeyEntry);
            if (alias is null)
                throw new InvalidOperationException("No private key entry found in the PFX.");

            var keyEntry = store.GetKey(alias);
            if (keyEntry?.Key is null)
                throw new InvalidOperationException("Private key could not be read from the PFX.");

            var certChain = store.GetCertificateChain(alias)
                                 .Select(x => x.Certificate)
                                 .ToArray();

            if (certChain.Length == 0)
                throw new InvalidOperationException("Certificate chain not found in the PFX.");

            return (keyEntry.Key, certChain);
        }
    }
}
