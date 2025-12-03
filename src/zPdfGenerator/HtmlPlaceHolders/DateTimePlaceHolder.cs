using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class DateTimePlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class DateTimePlaceHolder<T> : CultureBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The override global culture information.</param>
        public DateTimePlaceHolder(string name, Func<T, DateTime?> map, string stringFormat = "G", CultureInfo overrideGlobalCultureInfo = null) 
            : base(name, stringFormat, overrideGlobalCultureInfo)
        {
            Map = map;
        }

        /// <summary>
        /// Gets the map numeric.
        /// </summary>
        /// <value>The map numeric.</value>
        public Func<T, DateTime?> Map { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var dateTimeValue = Map(dataItem)?.ToString(StringFormat, OverrideGlobalCultureInfo ?? cultureInfo);
            SetTextValue(htmlNode, dateTimeValue, logger);
        }
    }
}
