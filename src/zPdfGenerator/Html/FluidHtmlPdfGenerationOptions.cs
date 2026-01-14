using System;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Options controlling HTML template rendering and PDF generation behavior.
    /// </summary>
    public sealed class FluidHtmlPdfGenerationOptions
    {
        /// <summary>
        /// Enables logging of rendered HTML at debug level. Defaults to true to preserve existing behavior.
        /// </summary>
        public bool LogRenderedHtml { get; set; } = true;

        /// <summary>
        /// Limits the number of characters logged for rendered HTML. Null logs the full template.
        /// </summary>
        public int? RenderedHtmlLogMaxLength { get; set; }

        /// <summary>
        /// Resource access policy used to restrict external resource loading during conversion.
        /// </summary>
        public HtmlResourceAccessPolicy? ResourceAccessPolicy { get; set; }

        internal void Validate()
        {
            if (RenderedHtmlLogMaxLength.HasValue && RenderedHtmlLogMaxLength.Value <= 0)
                throw new ArgumentOutOfRangeException(nameof(RenderedHtmlLogMaxLength), "RenderedHtmlLogMaxLength must be greater than zero.");
        }
    }
}
