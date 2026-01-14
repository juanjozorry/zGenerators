using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Orientation for bar charts.
    /// </summary>
    public enum BarChartOrientationEnum
    {
        /// <summary>
        /// Vertical chart (categories on Y axis).
        /// </summary>
        Vertical,

        /// <summary>
        /// Horizontal chart (categories on X axis).
        /// </summary>
        Horizontal
    }

    /// <summary>
    /// Placement for labels in bar charts.
    /// </summary>
    public enum LabelPlacementEnum
    {
        /// <summary>
        /// The label is placed inside the bar.
        /// </summary>
        Inside,

        /// <summary>
        /// The label is placed outside the bar.
        /// </summary>
        Outside
    }

    /// <summary>
    /// Helper class to render charts as SVG using OxyPlot. 
    /// We centralize Oxyplot usage here to avoid having it spread across the project.
    /// </summary>
    internal static class SvgChartRenderer
    {
        /// <summary>
        /// Generates a pie chart as SVG.
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="items">A collection of items.</param>
        /// <param name="label">The property to get the labels.</param>
        /// <param name="value">The value to render the pie chart.</param>
        /// <param name="insideLabelFormat">The inside label format. If null or empty, the label won't be rendered.</param>
        /// <param name="outsideLabelFormat">The outside label format. If null or empty, the label won't be rendered.</param>
        /// <param name="title">The title for the pie chart.</param>
        /// <param name="legend">The legend for the pie chart. If null, no legend will be rendered.</param>
        /// <param name="paletteHex">The palette of colors for the pie chart. If null or empty, automatic colors will be used.</param>
        /// <param name="width">The width for the pie chart.</param>
        /// <param name="height">The height for the pie chart.</param>
        /// <param name="culture">The culture for rendering the chart.</param>
        /// <returns>Returns the content of the chart rendered as SVG.</returns>
        /// <exception cref="ArgumentNullException">If a required parameter is missing.</exception>
        public static string GeneratePieSvg<T>(IEnumerable<T> items,
            Func<T, string> label, Func<T, double> value,
            string? insideLabelFormat = "{0:0.##}",
            string? outsideLabelFormat = "{1}",
            string? title = null, string? legend = null,
            IReadOnlyList<string>? paletteHex = null,
            int? width = 800, int? height = 450,
            CultureInfo? culture = null)
        {
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (label is null) throw new ArgumentNullException(nameof(label));
            if (value is null) throw new ArgumentNullException(nameof(value));

            var data = items
                .Select(i => (Label: label(i) ?? string.Empty, Value: value(i)))
                .Where(x => !string.IsNullOrWhiteSpace(x.Label))
                .ToList();

            PlotModel model = CreatePlotModel(title, legend, culture);

            var pie = new PieSeries
            {
                StrokeThickness = 0.5,
                InsideLabelPosition = 0.8,
                AngleSpan = 360,
                StartAngle = 0,
                InsideLabelFormat = insideLabelFormat ?? string.Empty,
                OutsideLabelFormat = outsideLabelFormat ?? string.Empty
            };

            for (int i = 0; i < data.Count; i++)
            {
                var d = data[i];
                var slice = new PieSlice(d.Label, d.Value);

                if (paletteHex is { Count: > 0 })
                {
                    var hex = paletteHex[i % paletteHex.Count];
                    slice.Fill = OxyColor.Parse(hex);
                }

                pie.Slices.Add(slice);
            }

            model.Series.Add(pie);

            return model.ExportPlot(width, height, culture);
        }

        /// <summary>
        /// Generates a bar chart as SVG.
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="items">A collection of items.</param>
        /// <param name="category">A function to get the categories for the chart.</param>
        /// <param name="value">The function to render the values for the chart.</param>
        /// <param name="fillColorHex">The fill color for the bars in hex format (e.g., "#FF0000").</param>
        /// <param name="chartOrientation">The orientation for the chart.</param>
        /// <param name="labelPlacement">The placement for the labels.</param>
        /// <param name="labelFormat">The label format. If null or empty provided,the label is not rendered.</param>
        /// <param name="title">The title for the pie chart.</param>
        /// <param name="legend">The legend for the pie chart. If null, no legend will be rendered.</param>
        /// <param name="width">The width for the pie chart.</param>
        /// <param name="height">The height for the pie chart.</param>
        /// <param name="culture">The culture for rendering the chart.</param>
        /// <returns>Returns the content of the chart rendered as SVG.</returns>
        /// <exception cref="ArgumentNullException">If a required parameter is missing.</exception>
        public static string GenerateBarChartSvg<T>(
            IEnumerable<T> items,
            Func<T, string> category,
            Func<T, double> value,
            string? fillColorHex = null,
            BarChartOrientationEnum? chartOrientation = BarChartOrientationEnum.Horizontal,
            LabelPlacementEnum? labelPlacement = LabelPlacementEnum.Inside,
            string? labelFormat = null, string? title = null, string? legend = null,
            int width = 800, int height = 450,
            CultureInfo? culture = null)
        {
            var list = items?.ToList() ?? throw new ArgumentNullException(nameof(items));
            
            PlotModel model = CreatePlotModel(title, legend, culture);

            var (catAxis, valAxis, xKey, yKey) = BuildBarAxes(chartOrientation);

            model.Axes.Add(catAxis);
            model.Axes.Add(valAxis);

            var series = new BarSeries
            {
                Title = legend,
                FillColor = fillColorHex is null ? OxyColors.Automatic : OxyColor.Parse(fillColorHex),
                LabelPlacement = labelPlacement switch
                {
                    LabelPlacementEnum.Outside => LabelPlacement.Outside,
                    LabelPlacementEnum.Inside => LabelPlacement.Inside,
                    _ => LabelPlacement.Inside
                },
                XAxisKey = xKey,
                YAxisKey = yKey,
                LabelFormatString = string.IsNullOrWhiteSpace(labelFormat) ? "{0}" : labelFormat,
            };

            foreach (var it in list)
            {
                catAxis.Labels.Add(category(it));
                series.Items.Add(new BarItem(value(it)));
            }

            model.Series.Add(series);

            return model.ExportPlot(width, height, culture);
        }

        /// <summary>
        /// Generates a grouped bar chart as SVG.
        /// </summary>
        /// <typeparam name="T">The type of the items</typeparam>
        /// <param name="items">A collection of items.</param>
        /// <param name="category">A function to get the categories for the chart.</param>
        /// <param name="value">The function to render the values for the chart.</param>
        /// <param name="seriesName">The function to render the series name.</param>
        /// <param name="chartOrientation">The orientation for the chart.</param>
        /// <param name="paletteHex">The palette for the bars in hex format (e.g., "#FF0000"). If not provided, an automatic palette will be used.</param>
        /// <param name="labelPlacement">The placement for the labels.</param>
        /// <param name="labelFormat">The label format. If null or empty provided,the label is not rendered.</param>
        /// <param name="title">The title for the pie chart.</param>
        /// <param name="legend">The legend for the pie chart. If null, no legend will be rendered.</param>
        /// <param name="width">The width for the pie chart.</param>
        /// <param name="height">The height for the pie chart.</param>
        /// <param name="culture">The culture for rendering the chart.</param>
        /// <returns>Returns the content of the chart rendered as SVG.</returns>
        /// <exception cref="ArgumentNullException">If a required parameter is missing.</exception>
        public static string GenerateGroupedBarChartSvg<T>(
            IEnumerable<T> items,
            Func<T, string> category,
            Func<T, string> seriesName,
            Func<T, double> value,
            IReadOnlyList<string>? paletteHex = null,
            BarChartOrientationEnum? chartOrientation = BarChartOrientationEnum.Horizontal, 
            LabelPlacementEnum? labelPlacement = LabelPlacementEnum.Inside,
            string? labelFormat = null, string? title = null, string? legend = null,
            int width = 800, int height = 450,
            CultureInfo? culture = null)
        {
            var list = items?.ToList() ?? throw new ArgumentNullException(nameof(items));

            List<string> categories = list.Select(category).Distinct().ToList();
            List<string> seriesNames = list.Select(seriesName).Distinct().ToList();

            var index = list
                .GroupBy(x => (Cat: category(x), Ser: seriesName(x)))
                .ToDictionary(g => g.Key, g => g.Sum(value));

            PlotModel model = CreatePlotModel(title, legend, culture);

            var (catAxis, valAxis, xKey, yKey) = BuildBarAxes(chartOrientation);

            model.Axes.Add(catAxis);
            model.Axes.Add(valAxis);

            foreach (var s in seriesNames)
            {
                var series = new BarSeries 
                { 
                    Title = s,
                    LabelPlacement = labelPlacement switch
                    {
                        LabelPlacementEnum.Outside => LabelPlacement.Outside,
                        LabelPlacementEnum.Inside => LabelPlacement.Inside,
                        _ => LabelPlacement.Inside
                    },
                    XAxisKey = xKey,
                    YAxisKey = yKey,
                    LabelFormatString = string.IsNullOrWhiteSpace(labelFormat) ? "{0}" : labelFormat,
                };
                series.FillColor = paletteHex?.Count > 0 ? OxyColor.Parse(paletteHex[seriesNames.IndexOf(s) % paletteHex.Count]) : OxyColors.Automatic;
                foreach (var c in categories)
                {
                    index.TryGetValue((c, s), out var v);
                    series.Items.Add(new BarItem(v));
                }

                model.Series.Add(series);
            }

            return model.ExportPlot(width, height, culture);
        }

        private const string CatAxisKey = "cat";
        private const string ValAxisKey = "val";

        private static (CategoryAxis catAxis, LinearAxis valAxis, string seriesXAxisKey, string seriesYAxisKey)
            BuildBarAxes(BarChartOrientationEnum? orientation = BarChartOrientationEnum.Horizontal)
        {
            if (orientation == BarChartOrientationEnum.Horizontal)
            {
                var cat = new CategoryAxis { Position = AxisPosition.Left, Key = CatAxisKey };
                var val = new LinearAxis
                {
                    Position = AxisPosition.Bottom,
                    Key = ValAxisKey,
                    MinimumPadding = 0,
                    AbsoluteMinimum = 0
                };

                // BarSeries expects:
                // X axis = value axis, Y axis = category axis
                return (cat, val, ValAxisKey, CatAxisKey);
            }
            else
            {
                // Rotated: draw category axis at Bottom but assign it as the series' Y axis (categories)
                var cat = new CategoryAxis { Position = AxisPosition.Bottom, Key = CatAxisKey };
                var val = new LinearAxis
                {
                    Position = AxisPosition.Left,
                    Key = ValAxisKey,
                    MinimumPadding = 0,
                    AbsoluteMinimum = 0
                };

                // Still: X axis = value axis, Y axis = category axis (by key)
                return (cat, val, ValAxisKey, CatAxisKey);
            }
        }


        private static PlotModel CreatePlotModel(string? title, string? legend, CultureInfo? culture)
        {
            var model = new PlotModel
            {
                Title = title ?? string.Empty,
                IsLegendVisible = !string.IsNullOrEmpty(legend),
                Culture = culture ?? CultureInfo.CurrentCulture
            };

            if (model.IsLegendVisible)
            {
                var legendDef = new Legend
                {
                    LegendTitle = legend ?? string.Empty,
                    LegendPosition = LegendPosition.RightTop,
                    LegendOrientation = LegendOrientation.Vertical,
                    LegendPlacement = LegendPlacement.Outside,
                };
                model.Legends.Add(legendDef);
            }

            return model;
        }
    }
}
