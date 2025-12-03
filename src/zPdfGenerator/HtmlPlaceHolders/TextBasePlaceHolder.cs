using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class TextBasePlaceHolder.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TextBasePlaceHolder<T> : BasePlaceHolder<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TextBasePlaceHolder{T}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public TextBasePlaceHolder(string name) 
            : base(name)
        {
        }

        /// <summary>
        /// Sets the text value.
        /// </summary>
        /// <param name="htmlNode">The HTML node.</param>
        /// <param name="textValue">The text value.</param>
        /// <param name="logger">The logger.</param>
        protected void SetTextValue(HtmlNode htmlNode, string textValue, ILogger logger)
        {
            if (textValue == null)
            {
                logger.LogDebug($"Text item {Name} skipped because no data is present");
                return;
            }

            var nodes = htmlNode.SelectNodes($".//*[(self::span or self::p or self::div or self::li) and @id='{Name}']");
            if (nodes?.Any() != true)
            {
                logger.LogInformation($"span/p '{Name}' not found on template");
                return;
            }

            foreach (var n in nodes)
            {
                n.ParentNode.ReplaceChild(htmlNode.OwnerDocument.CreateTextNode(textValue), n);
            }
        }

    }
}
