using System;
using System.Globalization;

namespace zPdfGenerator.Forms.FormPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="BasePlaceHolder{T}" />
    /// </summary>
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
        /// Processes the data.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public override string ProcessData(T dataItem, CultureInfo cultureInfo)
        {
            return Map(dataItem);
        }
    }
}
