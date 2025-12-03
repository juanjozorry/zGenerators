using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class NullableNumericPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.NumericPlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.NumericPlaceHolder{T}" />
    public class NullableNumericPlaceHolder<T> : NumericPlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullableNumericPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The override global culture information.</param>
        public NullableNumericPlaceHolder(string name, Func<T, decimal?> map, string defaultValue, string stringFormat = "N", CultureInfo overrideGlobalCultureInfo = null) 
            : base(name, map, stringFormat, overrideGlobalCultureInfo)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>The default value.</value>
        public string DefaultValue { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var nullableNumericValue = Map(dataItem)?.ToString(StringFormat, OverrideGlobalCultureInfo ?? cultureInfo) ?? DefaultValue;
            SetTextValue(htmlNode, nullableNumericValue, logger);
        }
    }
}
