using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Defines a contract for PDF post-processors.
    /// </summary>
    public interface IPostProcessor
    {
        /// <summary>
        /// Indicates whether this post-processor should be executed last in the processing chain. 
        /// </summary>
        bool LastPostProcessor { get; }

        /// <summary>
        /// Processes the specified PDF data and returns the resulting byte array.
        /// </summary>
        /// <param name="pdfData">The PDF file data to process, represented as a byte array. Cannot be null.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A byte array containing the processed PDF data.</returns>
        byte[] Process(byte[] pdfData, CancellationToken cancellationToken);
    }
}
