using iText.Forms;
using iText.Html2pdf;
using iText.Html2pdf.Attach.Impl;
using iText.Kernel.Pdf;
using iText.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace zPdfGenerator
{
    /// <summary>
    /// Enum Classification
    /// </summary>
    public enum Classification
    {
        /// <summary>
        /// The confidential
        /// </summary>
        Confidential,

        /// <summary>
        /// The internal
        /// </summary>
        Internal,

        /// <summary>
        /// The public
        /// </summary>
        Public
    }

    /// <summary>
    /// Class PdfHelpers. This class contains helpres methods
    /// </summary>
    public static class PdfHelpers
    {
        /// <summary>
        /// Checks the PDF number of signers.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="expectedSigners">The expected signers.</param>
        /// <returns><c>true</c> if the number of signers match, <c>false</c> otherwise.</returns>
        public static bool CheckPdfNumberOfSigners(byte[] fileContents, int expectedSigners)
        {
            if (fileContents is null || fileContents.Length == 0) throw new NullReferenceException($"{nameof(fileContents)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(fileContents))
            {
                using (var pdfReader = new PdfReader(readerStream))
                {
                    using (var document = new PdfDocument(pdfReader))
                    {
                        var signUtil = new SignatureUtil(document);
                        var names = signUtil.GetSignatureNames();

                        return names.Count() == expectedSigners;
                    }
                }
            }
        }

        /// <summary>
        /// Classifies the PDF.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="classification">The classification.</param>
        /// <param name="additionalValues">The additional values.</param>
        /// <returns>System.Byte[].</returns>
        public static byte[] ClassifyPdf(byte[] fileContents, Classification classification, IDictionary<string, string> additionalValues = null)
        {
            if (fileContents is null || fileContents.Length == 0) throw new NullReferenceException($"{nameof(fileContents)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(fileContents))
            {
                using (var pdfReader = new PdfReader(readerStream))
                {
                    using (var writerStream = new MemoryStream())
                    {
                        var props = new WriterProperties().AddXmpMetadata();

                        using (var pdfWriter = new PdfWriter(writerStream, props))
                        {
                            using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
                            {
                                var info = pdfDoc.GetDocumentInfo();
                                info.SetMoreInfo("Classification", GetClassification(classification));
                                info.SetMoreInfo("SI_DATA", GetSiData(classification));

                                if (additionalValues != null)
                                {
                                    info.SetMoreInfo(additionalValues);
                                }

                                pdfDoc.Close();
                                return writerStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Checks the PDF values.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns true if the Pdf matches, false otherwise.</returns>
        public static bool CheckPdfValue(byte[] fileContents, string key, string value)
        {
            if (fileContents is null || fileContents.Length == 0) throw new NullReferenceException($"{nameof(fileContents)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(fileContents))
            {
                using (var pdfReader = new PdfReader(readerStream))
                {
                    using (var pdfDoc = new PdfDocument(pdfReader))
                    {
                        var info = pdfDoc.GetDocumentInfo();

                        var savedValue = info.GetMoreInfo(key);
                        return savedValue == value;
                    }
                }
            }
        }

        /// <summary>
        /// Converts the specified Html template to PDF.
        /// </summary>
        /// <param name="htmlContents">The Html contents.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Returns the PDF as an array of bytes.</returns>
        public static async Task<byte[]> ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
        {
            if (htmlContents is null) throw new NullReferenceException($"{nameof(htmlContents)} parameter is mandatory");

            var properties = new ConverterProperties()
                .SetOutlineHandler(OutlineHandler.CreateStandardHandler())
                .SetBaseUri(basePath)
                .SetImmediateFlush(true);

            using (var auxStream = new MemoryStream())
            {
                using (var pdfWriter = new PdfWriter(auxStream))
                {
                    using (var pdfDocument = new PdfDocument(pdfWriter))
                    {
                        pdfDocument.SetCloseWriter(true);
                        pdfDocument.SetCloseReader(true);
                        pdfDocument.SetFlushUnusedObjects(true);

                        cancellationToken.ThrowIfCancellationRequested();

                        //TODO: cancel processing if cancellation happens
                        await Task.Run(() => HtmlConverter.ConvertToPdf(htmlContents, pdfDocument, properties), cancellationToken);

                        pdfDocument.Close();

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return auxStream.ToArray();
            }
        }

        /// <summary>
        /// Encrypts the PDF.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <param name="masterPassword">The master password.</param>
        /// <param name="userPassword">The user password.</param>
        /// <returns>Returns the contents of encrypted pdf.</returns>
        public static byte[] EncryptPdf(byte[] fileContents, string masterPassword, string userPassword)
        {
            if (fileContents is null || fileContents.Length == 0) throw new NullReferenceException($"{nameof(fileContents)} parameter is mandatory or needs data");
            if (masterPassword is null || masterPassword.Length == 0) throw new NullReferenceException($"{nameof(masterPassword)} parameter is mandatory or needs data");
            if (userPassword is null || userPassword.Length == 0) throw new NullReferenceException($"{nameof(userPassword)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(fileContents))
            {
                using (var pdfReader = new PdfReader(readerStream))
                {
                    using (var writerStream = new MemoryStream())
                    {
                        var userPasswordBytes = Encoding.ASCII.GetBytes(userPassword);
                        var masterPasswordBytes = Encoding.ASCII.GetBytes(masterPassword);
                        var props = new WriterProperties()
                                .SetStandardEncryption(userPasswordBytes, masterPasswordBytes, EncryptionConstants.ALLOW_PRINTING,
                                        EncryptionConstants.ENCRYPTION_AES_256 | EncryptionConstants.DO_NOT_ENCRYPT_METADATA);
                        using (var pdfWriter = new PdfWriter(writerStream, props))
                        {
                            using (var pdfDoc = new PdfDocument(pdfReader, pdfWriter))
                            {
                                pdfDoc.Close();
                                return writerStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the number of PDF pages.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.Int32.</returns>
        public static int GetNumberOfPdfPages(byte[] fileContents)
        {
            using (var stream = new MemoryStream(fileContents))
            {
                using (var pdfReader = new PdfReader(stream))
                {
                    using (var pdfDocument = new PdfDocument(pdfReader))
                    {
                        return pdfDocument.GetNumberOfPages();
                    }
                }
            }
        }
        /// <summary>
        /// Determines whether the specified PDF contents are a valid PDF.
        /// </summary>
        /// <param name="pdfContents">The PDF contents.</param>
        /// <returns><c>true</c> if the specified PDF contents are a valid PDF; otherwise, <c>false</c>.</returns>
        public static bool IsPDFHeader(this byte[] pdfContents)
        {
            var buffer = pdfContents.Take(5).ToArray();
            var header = Encoding.ASCII.GetString(buffer);
            return header.StartsWith("%PDF-");
        }

        internal static string GetClassification(Classification classification) =>
            classification switch
            {
                Classification.Confidential => "Confidential",
                Classification.Internal => "Internal",
                Classification.Public => "Public",
                _ => throw new Exception("Unknown Classification Type Requested")
            };

        internal static string GetSiData(Classification classification) =>
            classification switch
            {
                Classification.Confidential => ClassificationValues.Confidential,
                Classification.Internal => ClassificationValues.Internal,
                Classification.Public => ClassificationValues.Public,
                _ => throw new Exception("Unknown Classification Type Requested")
            };

        internal static class ClassificationValues
        {
            // Document classified as “Confidential” by application. (UserSID = “System”)
            public const string Confidential = "DataClass%2b9d401f75-6608-41d3-bd1f-efe1542cdc01=I%3D9d401f75-6608-41d3-bd1f-efe1542cdc01%26N%3DConfidential%26V%3D1.3%26U%3DSystem%26D%3DMidalaNET%26A%3DAssociated%26H%3DFalse";

            // Document classified as “Internal” by application. (UserSID = “System”)
            public const string Internal = "DataClass%2b5e2ccede-aa3d-4eba-b8ed-43e293c7fd2e=I%3d5e2ccede-aa3d-4eba-b8ed-43e293c7fd2e%26N%3dInternal%26V%3d1.3%26U%3dSystem%26D%3DMidalaNET%26A%3DAssociated%26H%3DFalse";

            // Document classified as “Public” by application. (UserSID = “System”)
            public const string Public = "DataClass%2b304a34c9-5b17-4e2a-bdc3-dec6a43f35e7=I%3d304a34c9-5b17-4e2a-bdc3-dec6a43f35e7%26N%3dPublic%26V%3d1.3%26U%3dSystem%26D%3DMidalaNET%26A%3DAssociated%26H%3DFalse";
        }
    }
}
