using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class SelfApplicableStylePlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.StylePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.StylePlaceHolder{T}" />
    public class SelfApplicableStylePlaceHolder<T> : StylePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StylePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mustAdd">The must add.</param>
        /// <param name="style">The style.</param>
        public SelfApplicableStylePlaceHolder(string name, Func<T, bool> mustAdd, string style) : base(name, mustAdd, style) { }

        /// <summary>
        /// Processes the node with the place holder.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode whose styles will change.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            if (MustAdd(dataItem))
            {
                if (string.IsNullOrWhiteSpace(Style))
                {
                    logger.LogDebug($"Style item {Name} skipped because no data is present");
                    return;
                }

                var styles = htmlNode.GetAttributeValue("style", null);
                var separator = styles == null ? null : "; ";
                htmlNode.SetAttributeValue("style", styles + separator + Style);
            }
        }
    }
}
