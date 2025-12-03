using System;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class NumericPlaceHolder.
    /// Implements the <see cref="BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BasePlaceHolder{T}" />
    public class NumericPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureNumericPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public NumericPlaceHolder(string name, Func<T, decimal?> map) 
            : base(name)
        {
            Map = map;
        }

        /// <summary>
        /// Gets the map numeric.
        /// </summary>
        /// <value>The map numeric.</value>
        public Func<T, decimal?> Map { get; }

        /// <summary>
        /// Converts the specified data item to its string representation using the provided culture and format
        /// settings.
        /// </summary>
        /// <param name="dataItem">The data item to be processed and converted to a string.</param>
        /// <param name="culture">The culture information to use for formatting the string representation.</param>
        /// <returns>A string representation of the mapped data item formatted according to the specified culture and format; or
        /// null if the data item cannot be mapped.</returns>
        public override object? ProcessValue(T dataItem, CultureInfo culture)
        {
            return Map(dataItem);
        }
    }
}
