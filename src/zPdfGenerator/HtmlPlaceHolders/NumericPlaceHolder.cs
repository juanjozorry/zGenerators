using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class NumericPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class NumericPlaceHolder<T> : CultureBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NumericPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The override global culture information.</param>
        public NumericPlaceHolder(string name, Func<T, decimal?> map, string stringFormat = "N", CultureInfo overrideGlobalCultureInfo = null) 
            : base(name, stringFormat, overrideGlobalCultureInfo)
        {
            Map = map;
        }

        /// <summary>
        /// Gets the map numeric.
        /// </summary>
        /// <value>The map numeric.</value>
        public Func<T, decimal?> Map { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var numericValue = Map(dataItem)?.ToString(StringFormat, OverrideGlobalCultureInfo ?? cultureInfo);
            SetTextValue(htmlNode, numericValue, logger);
        }
    }
}
