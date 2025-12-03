using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class BasePlaceHolder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BasePlaceHolder<T>
    {
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
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public abstract void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger);
    }
}
