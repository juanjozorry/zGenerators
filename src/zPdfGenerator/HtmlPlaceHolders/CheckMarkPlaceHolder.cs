using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class CheckMarkPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class CheckMarkPlaceHolder<T> : BasePlaceHolder<T>
    {
        private const string CHECKMARK = @"&#10004;";

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckMarkPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="check">The check.</param>
        public CheckMarkPlaceHolder(string name, Func<T, bool> check) : base(name)
        {
            this.Check = check;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, bool> Check { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            if (Check(dataItem))
            {
                var nodes = htmlNode.SelectNodes($".//*[(self::span or self::p or self::div or self::li) and @id='{Name}']");
                if (nodes?.Any() != true)
                {
                    logger.LogInformation($"span/p '{Name}' not found on template");
                    return;
                }

                foreach (var n in nodes)
                {
                    n.ParentNode.ReplaceChild(htmlNode.OwnerDocument.CreateTextNode(CHECKMARK), n);
                }
            }
        }
    }
}
