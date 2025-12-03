using System.Globalization;

namespace zPdfGenerator.FormPlaceHolders
{
    /// <summary>
    /// Class BasePlaceHolder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BasePlaceHolder<T>
    {
        /// <summary>
        /// The string format
        /// </summary>
        protected const string stringFormat = "N";

        /// <summary>
        /// Initializes a new instance of the <see cref="BasePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BasePlaceHolder(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Processes the data.
        /// </summary>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <returns>System.String.</returns>
        public abstract string ProcessData(T dataItem, CultureInfo cultureInfo);
    }
}
