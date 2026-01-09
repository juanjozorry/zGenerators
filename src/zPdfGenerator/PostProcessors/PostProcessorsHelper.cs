using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace zPdfGenerator.PostProcessors
{
    /// <summary>
    /// Provides helper methods for applying post-processing operations to PDF documents.
    /// </summary>
    internal static class PostProcessorsHelper
    {
        /// <summary>
        /// Applies a sequence of post-processing operations to the specified PDF document, optionally including a
        /// single final post processor.
        /// </summary>
        /// <remarks>Normal post processors are applied in sequence, followed by a single last post
        /// processor if present. If no post processors are provided, the original PDF is returned unchanged.</remarks>
        /// <param name="pdf">The PDF document to process, represented as a byte array.</param>
        /// <param name="processors">An enumerable collection of post processors to apply to the PDF. If <paramref name="processors"/> is <see
        /// langword="null"/>, no processing is performed.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the post-processing operations.</param>
        /// <returns>A byte array containing the processed PDF document after all post processors have been applied.</returns>
        /// <exception cref="InvalidOperationException">Thrown if more than one post processor in <paramref name="processors"/> is marked as a last post processor.</exception>
        public static byte[] RunPostProcessors(byte[] pdf, IEnumerable<IPostProcessor>? processors, CancellationToken ct)
        {
            if (processors is null) return pdf;

            var list = processors.ToList();

            // Separate last post processors from normal ones
            var last = list.Where(p => p.LastPostProcessor).ToList();
            var normal = list.Where(p => !p.LastPostProcessor).ToList();

            // Ensure only one last post processor
            if (last.Count > 1)
                throw new InvalidOperationException("Only one LastPostProcessor is allowed (typically the signer).");

            foreach (var p in normal)
                pdf = p.Process(pdf, ct);

            if (last.Count == 1)
                pdf = last[0].Process(pdf, ct);

            return pdf;
        }
    }
}
