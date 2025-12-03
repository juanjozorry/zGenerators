using System;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BasePlaceHolder{T}" />
    public class TextPlaceHolder<T> : BasePlaceHolder<T>
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
        /// Converts the specified data item to its display value using the provided culture information.
        /// </summary>
        /// <param name="dataItem">The data item to be converted to a display value.</param>
        /// <param name="culture">The culture information to use for formatting the display value.</param>
        /// <returns>An object representing the display value of the data item. Returns an empty string if the data item cannot
        /// be mapped.</returns>
        public override object? ProcessValue(T dataItem, CultureInfo culture)
        {
            return Map(dataItem);
        }
    }
}
