using System;
using System.Globalization;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class ClassedTextPlaceHolder<T> : TextPlaceHolder<T>
    {
        string CssClass { get; set; }
        string MatchingText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="matchingText">The matching text.</param>
        public ClassedTextPlaceHolder(string name, Func<T, string> map, string cssClass = null, string matchingText = null) : base(name, map)
        {
            CssClass = cssClass;
            MatchingText = matchingText;
        }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            base.ProcessNode(htmlNode, dataItem, cultureInfo, logger);

            var textValue = Map(dataItem);
            if (!string.IsNullOrWhiteSpace(CssClass) && !string.IsNullOrWhiteSpace(MatchingText) && !string.IsNullOrWhiteSpace(textValue))
            {
                if (textValue.Equals(MatchingText))
                {
                    htmlNode.AddClass(CssClass);
                }
            }
        }
    }
}
