using System;
using System.Globalization;

namespace zPdfGenerator.Forms.FormPlaceHolders
{
    internal class CheckboxPlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        public CheckboxPlaceHolder(string name, Func<T, bool> map) : base(name)
        {
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, bool> Map { get; }

        /// <summary>
        /// Processes the data.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public override string ProcessData(T dataItem, CultureInfo cultureInfo)
        {
            return Map(dataItem) ? "Yes" : "Off";
        }
    }
}
