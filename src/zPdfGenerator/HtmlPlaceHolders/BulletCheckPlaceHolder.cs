using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class BulletPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class BulletCheckPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BulletPlaceHolder{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public BulletCheckPlaceHolder(string name, Func<T, IEnumerable<string>> map) : base(name)
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
        /// <param name="dataItem">The item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var items = Map(dataItem);
            if (items?.Any() != true)
            {
                logger.LogDebug($"Bullet item {Name} skipped because no data is present");
                return;
            }

            var nodes = htmlNode.SelectNodes($".//*[self::div and @id='{Name}']");
            if (nodes?.Any() != true)
            {
                logger.LogInformation($"div '{Name}' not found on template");
                return;
            }

            foreach (var n in nodes)
            {
                n.ChildNodes.Clear();
                var ul = n.OwnerDocument.CreateElement("ul");
                n.AppendChild(ul);
                foreach (var item in items)
                {
                    var li = n.OwnerDocument.CreateElement("li");
                    ul.AppendChild(li);
                    var div = n.OwnerDocument.CreateElement("div");
                    li.AppendChild(div);
                    var divCheck = n.OwnerDocument.CreateElement("div");
                    div.AppendChild(divCheck);
                    divCheck.SetAttributeValue("class", "checkmark");
                    var spanText = n.OwnerDocument.CreateElement("span");
                    div.AppendChild(spanText);
                    spanText.AppendChild(n.OwnerDocument.CreateTextNode(item));
                }
            }
        }
    }
}
