using iText.Kernel.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Enum Classification
    /// </summary>
    public enum ClassificationEnum
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
    /// Provides post-processing functionality for document classification results.
    /// </summary>
    public sealed class DocumentClassifierPostProcessor : IPostProcessor
    {
        /// <summary>
        /// Initializes a new instance of the DocumentClassifierPostProcessor class with the specified classification
        /// and additional values.
        /// </summary>
        /// <param name="classification">The classification result to assign to the document. Specify null if no classification is available.</param>
        /// <param name="additionalValues">A dictionary containing additional key-value pairs to associate with the document. Can be null if no
        /// additional values are required.</param>
        public DocumentClassifierPostProcessor(ClassificationEnum? classification, IDictionary<string, string>? additionalValues = null)
        {
            Classification = classification;
            AdditionalValues = additionalValues;
        }

        /// <summary>
        /// Gets or sets the classification type for the entity.
        /// </summary>
        public ClassificationEnum? Classification { get; set; }

        /// <summary>
        /// Gets or sets a collection of additional key-value pairs associated with the object.
        /// </summary>
        /// <remarks>Use this property to store or retrieve custom metadata or extension values that are
        /// not represented by other properties. Keys are case-sensitive. The collection may be null if no additional
        /// values are present.</remarks>
        public IDictionary<string, string>? AdditionalValues { get; set; }

        /// <summary>
        /// Gets a value indicating whether the last post-processing operation was performed.
        /// </summary>
        public bool LastPostProcessor { get; } = false;

        private static readonly HashSet<string> ReservedMetadataKeys = new(StringComparer.Ordinal) { "Classification", "SI_DATA" };

        /// <summary>
        /// Processes the specified PDF data and returns the resulting byte array. Classifies the PDF.
        /// </summary>
        /// <param name="pdfData">The PDF file data to process, represented as a byte array. Cannot be null.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A byte array containing the processed PDF data.</returns>
        public byte[] Process(byte[] pdfData, CancellationToken cancellationToken)
        {
            if (pdfData is null || pdfData.Length == 0) throw new ArgumentNullException($"{nameof(pdfData)} parameter is mandatory or needs data");
            if (this.Classification is null) throw new ArgumentNullException($"{nameof(this.Classification)} parameter is mandatory or needs data");

            using (var readerStream = new MemoryStream(pdfData))
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

                                cancellationToken.ThrowIfCancellationRequested();

                                var cls = GetClassification(this.Classification.Value);

                                info.SetSubject($"Classification: {cls}");
                                info.SetKeywords($"classification={cls.ToLowerInvariant()}");
                                info.SetMoreInfo("Classification", cls);
                                info.SetMoreInfo("SI_DATA", GetSiData(this.Classification.Value));

                                cancellationToken.ThrowIfCancellationRequested();

                                if (this.AdditionalValues is not null)
                                {
                                    foreach (var kv in this.AdditionalValues)
                                    {
                                        cancellationToken.ThrowIfCancellationRequested();

                                        if (string.IsNullOrWhiteSpace(kv.Key))
                                            throw new ArgumentException("Metadata key cannot be null or empty.", nameof(this.AdditionalValues));

                                        if (ReservedMetadataKeys.Contains(kv.Key))
                                        {
                                            throw new InvalidOperationException(
                                                $"The metadata key '{kv.Key}' is reserved and cannot be overridden.");
                                        }

                                        info.SetMoreInfo(kv.Key, kv.Value ?? string.Empty);
                                    }
                                }

                                pdfDoc.Close();

                                cancellationToken.ThrowIfCancellationRequested();

                                return writerStream.ToArray();
                            }
                        }
                    }
                }
            }
        }

        internal static string GetClassification(ClassificationEnum classification) =>
            classification switch
            {
                ClassificationEnum.Confidential => "Confidential",
                ClassificationEnum.Internal => "Internal",
                ClassificationEnum.Public => "Public",
                _ => throw new Exception("Unknown Classification Type Requested")
            };

        internal static string GetSiData(ClassificationEnum classification) =>
            classification switch
            {
                ClassificationEnum.Confidential => ClassificationValues.Confidential,
                ClassificationEnum.Internal => ClassificationValues.Internal,
                ClassificationEnum.Public => ClassificationValues.Public,
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
