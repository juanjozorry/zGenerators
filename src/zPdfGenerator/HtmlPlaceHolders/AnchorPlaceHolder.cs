using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class AnchorHtmlTemplatePdfGeneratorItem.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class AnchorPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnchorPlaceHolder{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public AnchorPlaceHolder(string name, Func<T, string> map) : base(name)
        {
            Map = map;
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
        /// <param name="dataItem">The item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var url = Map(dataItem);
            if (url == null)
            {
                logger.LogDebug($"Anchor item {Name} skipped because no data is present");
                return;
            }

            var nodes = htmlNode.SelectNodes($".//*[self::a and @id='{Name}']");
            if (nodes?.Any() != true)
            {
                logger.LogInformation($"anchor '{Name}' not found on template");
                return;
            }

            foreach (var n in nodes)
            {
                var anchor = n.OwnerDocument.CreateElement("a");
                anchor.Attributes.Add("href", url);
                anchor.ChildNodes.Add(htmlNode.OwnerDocument.CreateTextNode(url));
                n.ParentNode.ReplaceChild(anchor, n);
            }
        }
    }
}
