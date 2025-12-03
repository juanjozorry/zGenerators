using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="BasePlaceHolder{T}" />
    public class CollectionPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public CollectionPlaceHolder(string name, Func<T, IEnumerable<object>> map) 
            : base(name)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, IEnumerable<object>> Map { get; }

        /// <summary>
        /// Processes the specified data item and returns the mapped value or an empty collection if no mapping is
        /// found.
        /// </summary>
        /// <param name="dataItem">The data item to process and map to a value. May be null depending on the implementation of the mapping
        /// function.</param>
        /// <param name="culture">The culture information to use for culture-specific processing. This parameter may influence formatting or
        /// conversion operations.</param>
        /// <returns>An object representing the mapped value for the specified data item, or an empty collection if the mapping
        /// yields no result.</returns>
        public override object? ProcessValue(T dataItem, CultureInfo culture)
        {
            return Map(dataItem);
        }
    }
}
