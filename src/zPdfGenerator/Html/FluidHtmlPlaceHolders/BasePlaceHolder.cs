using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// Class BasePlaceHolder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the BasePlaceHolder class with the specified name.
        /// </summary>
        /// <param name="name">The name to assign to the placeholder. Cannot be null or empty.</param>
        protected BasePlaceHolder(string name) => Name = name;
        
        /// <summary>
        /// Gets the name associated with the current instance.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Processes the specified data item and returns a value formatted according to the provided culture.
        /// </summary>
        /// <param name="dataItem">The data item to process. The type and meaning of this value depend on the implementation.</param>
        /// <param name="culture">The culture information to use for formatting or processing. Cannot be null.</param>
        /// <returns>An object representing the processed value of the data item, formatted as specified by the culture.</returns>
        public abstract object? ProcessValue(T dataItem, CultureInfo culture);
    }
}
