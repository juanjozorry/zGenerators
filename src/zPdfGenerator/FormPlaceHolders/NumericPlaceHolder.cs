using System;
using System.Globalization;

namespace zPdfGenerator.FormPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.FormPlaceHolders.CultureBasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.FormPlaceHolders.CultureBasePlaceHolder{T}" />
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
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, decimal?> Map { get; }

        /// <summary>
        /// Processes the data.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public override string ProcessData(T dataItem, CultureInfo cultureInfo)
        {
            var result = Map(dataItem);
            return result.HasValue ? result.Value.ToString(StringFormat, OverrideGlobalCultureInfo ?? cultureInfo) : null;
        }
    }
}
