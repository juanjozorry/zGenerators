using System;
using System.Globalization;

namespace zPdfGenerator.Forms.FormPlaceHolders
{
    /// <summary>
    /// Class DateTimePlaceHolder.
    /// Implements the <see cref="CultureBasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="CultureBasePlaceHolder{T}" />
    public class DateTimePlaceHolder<T> : CultureBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The override global culture information.</param>
        public DateTimePlaceHolder(string name, Func<T, DateTime?> map, string stringFormat = "G", CultureInfo? overrideGlobalCultureInfo = null) 
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
        /// Processes the data.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public override string ProcessData(T dataItem, CultureInfo cultureInfo)
        {
            var result = Map(dataItem);
            return result.HasValue ? result.Value.ToString(StringFormat, OverrideGlobalCultureInfo ?? cultureInfo) : string.Empty;
        }
    }
}
