using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class RawHtmlPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class RawHtmlPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RawHtmlPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public RawHtmlPlaceHolder(string name, Func<T, string> map)
            : base(name)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, string> Map { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var rawHtmlValue = Map(dataItem);

            if (string.IsNullOrWhiteSpace(rawHtmlValue))
            {
                logger.LogDebug($"Raw html item {Name} skipped because no data is present");
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
                var newNode = HtmlNode.CreateNode(rawHtmlValue);
                n.ParentNode.ReplaceChild(newNode, n);
            }
        }
    }
}
