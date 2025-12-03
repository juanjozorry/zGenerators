using iText.Html2pdf;
using iText.Html2pdf.Attach.Impl;
using iText.Kernel.Pdf;
using System;
using System.IO;
using System.Threading;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Defines a contract for converting HTML content to PDF format.
    /// </summary>
    /// <remarks>Implementations of this interface provide methods for generating PDF documents from HTML
    /// input. The specific conversion capabilities and supported features may vary depending on the
    /// implementation.</remarks>
    public interface IHtmlToPdfConverter
    {
        /// <summary>
        /// Converts the specified Html template to PDF.
        /// </summary>
        /// <param name="htmlContents">The Html contents.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Returns the PDF as an array of bytes.</returns>
        byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Provides functionality to convert HTML content to PDF format.
    /// </summary>
    /// <remarks>Use this class to generate PDF documents from HTML templates, specifying the HTML content and
    /// an optional base path for resolving relative resources. The conversion process supports cancellation via a
    /// cancellation token. This class is typically used in scenarios where server-side PDF generation from dynamic or
    /// static HTML is required.</remarks>
    public class HtmlToPdfConverter : IHtmlToPdfConverter
    {
        /// <summary>
        /// Converts the specified Html template to PDF.
        /// </summary>
        /// <param name="htmlContents">The Html contents.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>Returns the PDF as an array of bytes.</returns>
        public byte[] ConvertHtmlToPDF(string htmlContents, string basePath, CancellationToken cancellationToken)
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

                        HtmlConverter.ConvertToPdf(htmlContents, pdfDocument, properties);

                        pdfDocument.Close();

                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                return auxStream.ToArray();
            }
        }
    }
}
