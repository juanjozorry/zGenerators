using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class StylePlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class StylePlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StylePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mustAdd">The must add.</param>
        /// <param name="style">The style.</param>
        public StylePlaceHolder(string name, Func<T, bool> mustAdd, string style) : base(name)
        {
            MustAdd = mustAdd;
            Style = style;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, bool> MustAdd { get; }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public string Style { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
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

                var nodes = htmlNode.SelectNodes($".//*[(self::section or self::table or self::span or self::div or self::tr or self::td or self::article) and @id='{Name}']");
                if (nodes?.Any() != true)
                {
                    logger.LogInformation($"tag '{Name}' not found on template");
                    return;
                }

                foreach (var n in nodes)
                {
                    var styles = n.GetAttributeValue("style", null);
                    var separator = styles == null ? null : "; ";
                    n.SetAttributeValue("style", styles + separator + Style);
                }
            }
        }
    }
}
