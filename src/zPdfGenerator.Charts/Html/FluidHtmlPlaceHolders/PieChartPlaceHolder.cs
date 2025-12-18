using System;
using System.Collections.Generic;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// This placeholder is used to render a pie chart in HTML as SVG.
    /// </summary>
    public class PieChartPlaceHolder<TBase, TItem> : BasePlaceHolder<TBase>
    {
        private readonly CultureInfo? overrideGlobalCultureInfo;
        private readonly Func<TBase, IEnumerable<TItem>> map;
        private readonly Func<TItem, string> label;
        private readonly Func<TItem, double> value;
        private readonly string title;
        private readonly string? legend;
        private readonly string? insideLabelFormat;
        private readonly string? outsideLabelFormat;
        private readonly IReadOnlyList<string>? paletteHex;

        /// <summary>
        /// Initializes a new instance of the <see cref="PieChartPlaceHolder{TBase, TItem}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="label">A function to determine the label</param>
        /// <param name="value">A function to determine the value</param>
        /// <param name="title">The title for the pie</param>
        /// <param name="legend">A legend</param>
        /// <param name="insideLabelFormat">The format for the inside label.</param>
        /// <param name="outsideLabelFormat">The format for the outside label.</param>
        /// <param name="paletteHex">The palette values</param>
        /// <param name="overrideGlobalCultureInfo">A culture if the general culture needs to be overriden</param>
        public PieChartPlaceHolder(string name, Func<TBase, IEnumerable<TItem>> map, Func<TItem, string> label, Func<TItem, double> value, string title, string? legend = null,
            string? insideLabelFormat = null, string? outsideLabelFormat = null, IReadOnlyList<string>? paletteHex = null, CultureInfo? overrideGlobalCultureInfo = null) : base(name)
        {
            this.overrideGlobalCultureInfo = overrideGlobalCultureInfo;
            this.map = map;
            this.label = label;
            this.value = value;
            this.title = title;
            this.legend = legend;
            this.insideLabelFormat = insideLabelFormat;
            this.outsideLabelFormat = outsideLabelFormat;
            this.paletteHex = paletteHex;
        }

        /// <summary>
        /// Converts the specified data item to a chart representation in SVG text format using the provided culture settings.
        /// You must create a div over this SVG to make it responsive.
        /// <div class="z-chart">
        ///  {{ chartSvg | raw }}
        /// </div>
        /// </summary>
        /// <param name="dataItem">The data item to be processed and converted to a string.</param>
        /// <param name="culture">The culture information to use for formatting the string representation.</param>
        /// <returns>A string representation of the pie chart in SVG format; or null if the data item cannot be mapped.</returns>
        public override object? ProcessValue(TBase dataItem, CultureInfo culture)
        {
            IEnumerable<TItem> data = this.map(dataItem);
            if (data is null) return null;

            var svg = SvgChartRenderer.PieSvg(data, this.label, this.value, this.insideLabelFormat, this.outsideLabelFormat, this.title, this.legend, 
                paletteHex: this.paletteHex, culture: this.overrideGlobalCultureInfo ?? culture);

            return svg
                .RemoveXmlHeaders()
                .MakeResponsive();
        }
    }
}
