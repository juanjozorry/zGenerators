using iText.Kernel.Crypto;
using iText.Signatures;
using System;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Represents configuration options for applying a digital signature to a PDF document, including signature field
    /// details, cryptographic settings, and optional visible signature placement.
    /// </summary>
    /// <remarks>Use this class to specify parameters such as the password for the signing certificate, the
    /// name of the signature field, the digest algorithm, and whether the signature should be appended to the document.
    /// If a visible signature is required, set the relevant properties to define its appearance and location. The
    /// options also allow specifying the reason and location for the signature, as well as the cryptographic standard
    /// to use. All properties are immutable after construction.</remarks>
    public sealed class PdfSignatureOptions
    {
        /// <summary>
        /// Gets the password used to protect the PFX (Personal Information Exchange) certificate file.
        /// </summary>
        public string PfxPassword { get; } = "";

        /// <summary>
        /// Gets the name of the field associated with the signature.
        /// </summary>
        public string FieldName { get; } = "Signature1";

        /// <summary>
        /// Gets the name of the digest algorithm used for cryptographic operations.
        /// </summary>
        public string DigestAlgorithm { get; } = DigestAlgorithms.SHA256;

        /// <summary>
        /// Gets a value indicating whether data is appended to the existing content rather than overwriting it.
        /// </summary>
        public bool AppendMode { get; } = true;

        // Visible signature (optional)
        /// <summary>
        /// Gets a value indicating whether the signature is visible to the user.
        /// </summary>
        public bool Visible { get; } = false;

        /// <summary>
        /// Gets the page number in which the signature will be present in a paginated result set.
        /// </summary>
        public int PageNumber { get; } = 1;

        /// <summary>
        /// Gets the X coordinate value.
        /// </summary>
        public float X { get; } = 36;

        /// <summary>
        /// Gets the Y-coordinate value.
        /// </summary>
        public float Y { get; } = 36;

        /// <summary>
        /// Gets the width of the signature.
        /// </summary>
        public float Width { get; } = 200;

        /// <summary>
        /// Gets the height of the signature.
        /// </summary>
        public float Height { get; } = 60;

        /// <summary>
        /// Gets the reason for the signature.
        /// </summary>
        public string? Reason { get; }

        /// <summary>
        /// Gets the location for the signature.
        /// </summary>
        public string? Location { get; }

        /// <summary>
        /// Gets the cryptographic standard used for signing PDF documents.
        /// </summary>
        /// <remarks>Use this property to determine whether the PDF signer applies the CMS (Cryptographic
        /// Message Syntax) or CAdES (CMS Advanced Electronic Signatures) standard when creating digital signatures. The
        /// selected standard affects compatibility with signature validation software and compliance with regulatory
        /// requirements.</remarks>
        public PdfSigner.CryptoStandard CryptoStandard { get; } = PdfSigner.CryptoStandard.CMS;

        /// <summary>
        /// This password is used in case of provided PDF is password protected.
        /// </summary>
        public string? ExistingPdfPassword { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfSignatureOptions"/> class with the specified parameters.
        /// </summary>
        /// <param name="pfxPassword">The password used to protect the PFX (Personal Information Exchange) certificate file.</param>
        /// <param name="fieldName">The name of the field associated with the signature.</param>
        /// <param name="digestAlgorithm">The name of the digest algorithm used for cryptographic operations.</param>
        /// <param name="appendMode">Value indicating whether data is appended to the existing content rather than overwriting it.</param>
        /// <param name="visible">Value indicating whether the signature is visible to the user.</param>
        /// <param name="pageNumber">Page number in which the signature will be present in a paginated result set.</param>
        /// <param name="x">The X coordinate value.</param>
        /// <param name="y">The Y coordinate value.</param>
        /// <param name="width">Width of the signature.</param>
        /// <param name="height">Height of the signature</param>
        /// <param name="reason">Reason for the signature.</param>
        /// <param name="location">Location for the signature.</param>
        /// <param name="existingPdfPassword">Password used in case of the provided PDF is password protected.</param>
        /// <param name="cryptoStandard">The cryptographic standard used for signing PDF documents.</param>
        /// <exception cref="ArgumentNullException">Throws an exception it the PFX certificate is not present.</exception>
        public PdfSignatureOptions(
           string pfxPassword,
           string fieldName = "Signature1",
           string digestAlgorithm = DigestAlgorithms.SHA256,
           bool appendMode = true,
           bool visible = false,
           int pageNumber = 1,
           float x = 36,
           float y = 36,
           float width = 200,
           float height = 60,
           string? reason = null,
           string? location = null,
           string? existingPdfPassword = null,
           PdfSigner.CryptoStandard cryptoStandard = PdfSigner.CryptoStandard.CMS)
        {
            PfxPassword = pfxPassword ?? throw new ArgumentNullException(nameof(pfxPassword));
            FieldName = fieldName;
            DigestAlgorithm = digestAlgorithm;
            AppendMode = appendMode;
            Visible = visible;
            PageNumber = pageNumber;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Reason = reason;
            Location = location;
            ExistingPdfPassword = existingPdfPassword;
            CryptoStandard = cryptoStandard;
        }
    }
}
