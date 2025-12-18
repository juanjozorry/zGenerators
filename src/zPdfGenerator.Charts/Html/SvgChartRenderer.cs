using OxyPlot;
using OxyPlot.Legends;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using zPdfGenerator.Globalization;

namespace zPdfGenerator.Html
{
    internal static class SvgChartRenderer
    {
       public static string PieSvg<T>(IEnumerable<T> items,
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

            var model = new PlotModel
            {
                Title = title ?? string.Empty,
                IsLegendVisible = !string.IsNullOrEmpty(legend),
                Culture = culture ?? CultureInfo.CurrentCulture
            };

            var legendDef = new Legend
            {
                LegendTitle = legend ?? string.Empty,
                LegendPosition = LegendPosition.RightTop,
                LegendOrientation = LegendOrientation.Vertical,
                LegendPlacement = LegendPlacement.Outside,
            };
            model.Legends.Add(legendDef);

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

            using (CultureScope.Use(culture))
            {
                var exporter = new SvgExporter
                {
                    Width = width ?? 800,
                    Height = height ?? 450,
                    IsDocument = false,
                };

                using var ms = new MemoryStream();
                exporter.Export(model, ms);
                return Encoding.UTF8.GetString(ms.ToArray());
            }
        }
    }
}
