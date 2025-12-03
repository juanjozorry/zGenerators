using System.Globalization;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class CultureBasePlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public abstract class CultureBasePlaceHolder<T> : TextBasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CultureBasePlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="overrideGlobalCultureInfo">The override global culture information.</param>
        public CultureBasePlaceHolder(string name, string stringFormat, CultureInfo overrideGlobalCultureInfo) 
            : base(name)
        {
            StringFormat = stringFormat;
            OverrideGlobalCultureInfo = overrideGlobalCultureInfo;
        }

        /// <summary>
        /// Gets the string format.
        /// </summary>
        /// <value>The string format.</value>
        public string StringFormat { get; }

        /// <summary>
        /// Gets the override culture information.
        /// </summary>
        /// <value>The override culture information.</value>
        public CultureInfo OverrideGlobalCultureInfo { get; }
    }
}
