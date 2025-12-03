using System;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class NullableNumericPlaceHolder.
    /// Implements the <see cref="CultureNumericPlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="CultureNumericPlaceHolder{T}" />
    public class CultureNumericAndTextPlaceHolder<T> : CultureBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureNumericAndTextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The culture info to override the general</param>
        public CultureNumericAndTextPlaceHolder(string name, Func<T, NumericAndTextValue> map, string stringFormat = "N", CultureInfo? overrideGlobalCultureInfo = null) 
            : base(name, stringFormat, overrideGlobalCultureInfo)
        {
            Map = map;
        }

        /// <summary>
        /// Gets the map numeric.
        /// </summary>
        /// <value>The map numeric.</value>
        public Func<T, NumericAndTextValue> Map { get; }

        /// <summary>
        /// Converts the specified data item to a formatted string representation that combines its numeric and textual
        /// values, using the specified culture for formatting.
        /// </summary>
        /// <param name="dataItem">The data item to process and convert to a formatted string.</param>
        /// <param name="culture">The culture information to use for formatting the numeric value.</param>
        /// <returns>A string containing the formatted numeric value and associated text, separated by a space; or null if the
        /// numeric value is not available.</returns>
        public override object? ProcessValue(T dataItem, CultureInfo culture)
        {
            var result = Map(dataItem);
            var numericValue = result?.NumericValue?.ToString(StringFormat, OverrideGlobalCultureInfo ?? culture);
            return numericValue != null ? $"{numericValue} {result?.TextValue}" : null;
        }
    }

    /// <summary>
    /// Class NumericAndTextValue.
    /// </summary>
    public class NumericAndTextValue
    {
        /// <summary>
        /// Gets or sets the numeric value.
        /// </summary>
        /// <value>The numeric value.</value>
        public decimal? NumericValue { get; set; }

        /// <summary>
        /// Gets or sets the text value.
        /// </summary>
        /// <value>The text value.</value>
        public string TextValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericAndTextValue"/> class.
        /// </summary>
        /// <param name="numeric">The numeric.</param>
        /// <param name="text">The text.</param>
        public NumericAndTextValue(decimal? numeric, string text)
        {
            this.NumericValue = numeric;
            this.TextValue = text;
        }
    }
}
