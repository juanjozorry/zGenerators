using iText.Kernel.Pdf;
using System;
using System.IO;
using System.Text;
using System.Threading;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Provides functionality to apply password protection to documents or data after initial processing.
    /// </summary>
    public sealed class PasswordProtectPostProcessor : IPostProcessor
    {
        /// <summary>
        /// Initializes a new instance of the PasswordProtectPostProcessor class with the specified master and user
        /// passwords.
        /// </summary>
        /// <param name="masterPassword">The password that grants full access to the protected document. Cannot be null or empty.</param>
        /// <param name="userPassword">The password that grants limited access to the protected document. Cannot be null or empty.</param>
        public PasswordProtectPostProcessor(string masterPassword, string userPassword)
        {
            this.MasterPassword = masterPassword;
            this.UserPassword = userPassword;
        }

        /// <summary>
        /// Gets or sets the master password used for authentication or encryption purposes.
        /// </summary>
        public string? MasterPassword { get; }

        /// <summary>
        /// Gets or sets the password associated with the user.
        /// </summary>
        public string? UserPassword { get; }

        /// <summary>
        /// Gets a value indicating whether the last post-processing operation was performed.
        /// </summary>
        public bool LastPostProcessor { get; } = false;

        /// <summary>
        /// Processes the specified PDF data and returns the resulting byte array. Protects the PDF with a password.
        /// </summary>
        /// <param name="pdfData">The PDF file data to process, represented as a byte array. Cannot be null.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A byte array containing the processed PDF data.</returns>
        public byte[] Process(byte[] pdfData, CancellationToken cancellationToken)
        {
            if (pdfData is null || pdfData.Length == 0) throw new ArgumentNullException($"{nameof(pdfData)} parameter is mandatory or needs data");
            if (string.IsNullOrEmpty(this.MasterPassword)) throw new ArgumentNullException($"{nameof(this.MasterPassword)} parameter is mandatory or needs data");
            if (string.IsNullOrEmpty(this.UserPassword)) throw new ArgumentNullException($"{nameof(this.UserPassword)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(pdfData))
            {
                using (var pdfReader = new PdfReader(readerStream))
                {
                    using (var writerStream = new MemoryStream())
                    {
                        var userPasswordBytes = Encoding.ASCII.GetBytes(this.UserPassword);
                        var masterPasswordBytes = Encoding.ASCII.GetBytes(this.MasterPassword);
                        var props = new WriterProperties()
                                .SetStandardEncryption(userPasswordBytes, masterPasswordBytes, EncryptionConstants.ALLOW_PRINTING,
                                        EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);

                        cancellationToken.ThrowIfCancellationRequested();

                        using (var pdfWriter = new PdfWriter(writerStream, props))
                        {
                            using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
                            {
                                pdfDoc.Close();

                                cancellationToken.ThrowIfCancellationRequested();

                                return writerStream.ToArray();
                            }
                        }
                    }
                }
            }
        }
    }
}
