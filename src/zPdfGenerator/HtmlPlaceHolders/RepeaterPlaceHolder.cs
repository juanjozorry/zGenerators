using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{
    /// <summary>
    /// Class TextPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TChild">The type of the t child.</typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class RepeaterPlaceHolder<T, TChild> : BasePlaceHolder<T>
    {
        private string template;
        private Func<T, IEnumerable<TChild>> items;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="template">The template.</param>
        /// <param name="items">The items.</param>
        /// <param name="map">The map.</param>
        public RepeaterPlaceHolder(string name, string template, Func<T, IEnumerable<TChild>> items, Func<T, IEnumerable<BasePlaceHolder<TChild>>> map) : base(name)
        {
            this.template = template;
            this.items = items;
            this.Map = map;
        }

        /// <summary>
        /// Gets the map.
        /// </summary>
        /// <value>The map.</value>
        public Func<T, IEnumerable<BasePlaceHolder<TChild>>> Map { get; }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            var placeHolders = Map(dataItem);
            if (placeHolders?.Any() != true)
            {
                logger.LogDebug($"Repeater item {Name} skipped because no data is present");
                return;
            }

            var placeHolderNode = htmlNode.SelectSingleNode($".//*[(self::div or self::tbody or self::table) and @id='{Name}']");
            var templateNode = htmlNode.SelectSingleNode($".//*[(self::div or self::tr) and @id='{template}']");
            if (placeHolderNode == null)
            {
                logger.LogInformation($"Element div '{Name}' not found on template");
                return;
            }
            if (templateNode == null)
            {
                logger.LogInformation($"Element div '{template}' not found on template");
                return;
            }

            foreach (var item in this.items?.Invoke(dataItem) ?? Enumerable.Empty<TChild>())
            {
                var cloned = templateNode.CloneNode(true); // Clones the template to populate with data
                placeHolderNode.AppendChild(cloned); // Attaches the cloned item to the DOM

                foreach (var placeHolder in placeHolders)
                {
                    try
                    {
                        placeHolder.ProcessNode(cloned, item, cultureInfo, logger);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning($"Error trying to replace place holder {placeHolder.Name}: {ex}");
                    }
                }
            }

            templateNode.Remove();
        }
    }
}
