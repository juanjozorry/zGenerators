using System;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class DateTimePlaceHolder.
    /// Implements the <see cref="BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BasePlaceHolder{T}" />
    public class CultureDateTimePlaceHolder<T> : CultureBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureDateTimePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The culture info to override the general</param>
        public CultureDateTimePlaceHolder(string name, Func<T, DateTime?> map, string stringFormat = "G", CultureInfo? overrideGlobalCultureInfo = null) 
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
        /// Converts the specified data item to its string representation using the provided culture and format
        /// settings.
        /// </summary>
        /// <param name="dataItem">The data item to be processed and converted to a string.</param>
        /// <param name="culture">The culture information to use when formatting the string representation.</param>
        /// <returns>A string representation of the mapped value of the data item, formatted according to the specified culture
        /// and format. Returns an empty string if the mapped value is null.</returns>
        public override object? ProcessValue(T dataItem, CultureInfo culture)
        {
            return Map(dataItem)?.ToString(StringFormat, OverrideGlobalCultureInfo ?? culture);
        }
    }
}
