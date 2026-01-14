using System;
using System.Collections.Generic;
using System.Globalization;

namespace zPdfGenerator.Html.FluidHtmlPlaceHolders
{
    /// <summary>
    /// This class holds configuration for bar chart rendering.
    /// </summary>
    public class BarChartConfig
    {
        /// <summary>
        /// The title for the bar chart.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The optional legend for the bar chart.
        /// </summary>
        public string? Legend { get; set; }

        /// <summary>
        /// The list of colors for the palette in hex format (e.g., "#FF5733"). If not provided, a default palette will be used.
        /// </summary>
        public IReadOnlyList<string>? PaletteHex { get; set; }

        /// <summary>
        /// The fill color in hex format (e.g., "#FF5733") for all bars. If set, this color overrides the palette.
        /// </summary>
        public string? FillColorHex { get; set; }

        /// <summary>
        /// The chart orientation.
        /// </summary>
        public BarChartOrientationEnum ChartOrientation { get; set; }

        /// <summary>
        /// The label placement.
        /// </summary>
        public LabelPlacementEnum LabelPlacement { get; set; }

        /// <summary>
        /// The label format. If null, no label is rendered.
        /// </summary>
        public string? LabelFormat { get; set; }
    }
    /// <summary>
    /// This placeholder is used to render a pie chart in HTML as SVG.
    /// </summary>
    public class BarChartPlaceHolder<TBase, TItem> : BasePlaceHolder<TBase>
    {
        private readonly CultureInfo? overrideGlobalCultureInfo;
        private readonly Func<TBase, IEnumerable<TItem>> map;
        private readonly Func<TItem, string> label;
        private readonly Func<TItem, double> value;
        private readonly BarChartConfig? configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="BarChartPlaceHolder{TBase, TItem}"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="label">A function to determine the label</param>
        /// <param name="value">A function to determine the value</param>
        /// <param name="configuration">The configuration for for the pie chart.</param>
        /// <param name="overrideGlobalCultureInfo">A culture if the general culture needs to be overriden</param>
        public BarChartPlaceHolder(string name, Func<TBase, IEnumerable<TItem>> map, Func<TItem, string> label, Func<TItem, double> value, BarChartConfig? configuration = null, CultureInfo? overrideGlobalCultureInfo = null) : base(name)
        {
            this.overrideGlobalCultureInfo = overrideGlobalCultureInfo;
            this.map = map;
            this.label = label;
            this.value = value;
            this.configuration = configuration;
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

            return SvgChartRenderer.GenerateBarChartSvg(data, this.label, this.value, this.configuration?.FillColorHex, this.configuration?.ChartOrientation, this.configuration?.LabelPlacement, this.configuration?.LabelFormat, 
                this.configuration?.Title, this.configuration?.Legend, culture: this.overrideGlobalCultureInfo ?? culture);
        }
    }
}
