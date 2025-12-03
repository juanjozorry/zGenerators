using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class HideStylePlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class HideStylePlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HideHtmlPlaceHolder{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="hide">The hide.</param>
        public HideStylePlaceHolder(string name, Func<T, bool> hide) : base(name)
        {
            Hide = hide;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, bool> Hide { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            if (Hide(dataItem))
            {
                var nodes = htmlNode.SelectNodes($".//*[self::style and @id_style='{Name}']");
                if (nodes?.Any() != true)
                {
                    logger.LogInformation($"style '{Name}' not found on template");
                    return;
                }

                foreach (var n in nodes)
                {
                    n.Remove();
                }
            }
        }
    }
}
