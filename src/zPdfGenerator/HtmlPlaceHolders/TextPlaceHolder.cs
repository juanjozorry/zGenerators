using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class TextPlaceHolder<T> : TextBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public TextPlaceHolder(string name, Func<T, string> map) : base(name)
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
            var textValue = Map(dataItem);
            SetTextValue(htmlNode, textValue, logger);
        }
    }
}
