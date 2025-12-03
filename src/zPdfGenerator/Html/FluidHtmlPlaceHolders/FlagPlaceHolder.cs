using System;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class AnchorHtmlTemplatePdfGeneratorItem.
    /// Implements the <see cref="BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BasePlaceHolder{T}" />
    public class FlagPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlagPlaceHolder{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public FlagPlaceHolder(string name, Func<T, bool> map) : base(name)
        {
            Map = map;
        }
        
        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, bool> Map { get; }

        /// <summary>
        /// Converts the specified value to an object using the provided culture information.
        /// </summary>
        /// <param name="data">The value to convert.</param>
        /// <param name="culture">The culture information to use during the conversion.</param>
        /// <returns>An object that represents the converted value.</returns>
        public override object? ProcessValue(T data, CultureInfo culture)
        {
            return Map(data);
        }
    }
}
