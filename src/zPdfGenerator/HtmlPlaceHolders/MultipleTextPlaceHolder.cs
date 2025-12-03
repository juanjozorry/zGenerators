using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class MultipleTextPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public MultipleTextPlaceHolder(string name, Func<T, IEnumerable<string>> map) : base(name)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, IEnumerable<string>> Map { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var items = Map(dataItem);
            if (items?.Any() != true)
            {
                logger.LogDebug($"Multiple text item {Name} skipped because no data is present");
                return;
            }

            var nodes = htmlNode.SelectNodes($".//*[(self::span or self::p or self::div or self::li) and @id='{Name}']");
            if (nodes?.Any() != true)
            {
                logger.LogInformation($"Element '{Name}' not found on template");
                return;
            }

            foreach (var node in nodes)
            {
                node.ChildNodes.Clear();

                foreach (var item in items)
                {
                    var div = node.OwnerDocument.CreateElement("div");
                    node.AppendChild(div);
                    node.AppendChild(node.OwnerDocument.CreateTextNode(item));
                }
            }
        }
    }
}
