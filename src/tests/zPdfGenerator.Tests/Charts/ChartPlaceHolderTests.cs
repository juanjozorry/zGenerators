using System;
using System.Globalization;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;

namespace zPdfGenerator.Tests.Charts
{
    public class ChartPlaceHolderTests
    {
        private sealed record PieRow(string Label, double Value);
        private sealed record BarRow(string Category, double Value);
        private sealed record GroupRow(string Category, string Series, double Value);

        [Fact]
        public void PieChartPlaceHolder_ReturnsNull_WhenMapReturnsNull()
        {
            var placeholder = new PieChartPlaceHolder<object, PieRow>(
                name: "pie",
                map: _ => null!,
                label: x => x.Label,
                value: x => x.Value);

            var result = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void BarChartPlaceHolder_UsesFillColor()
        {
            var rows = new[] { new BarRow("Jan", 10) };
            var config = new BarChartConfig { FillColorHex = "#3366cc" };
            var placeholder = new BarChartPlaceHolder<object, BarRow>(
                name: "bar",
                map: _ => rows,
                label: x => x.Category,
                value: x => x.Value,
                configuration: config);

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("<svg", svg);
            Assert.Contains(HexToRgb("#3366cc"), svg);
        }

        [Fact]
        public void GroupedBarChartPlaceHolder_IncludesSeriesNamesAndLegend()
        {
            var rows = new[]
            {
                new GroupRow("Jan", "2024", 10),
                new GroupRow("Jan", "2025", 12)
            };

            var config = new GroupedBarChartConfig
            {
                Legend = "Years",
                Title = "Grouped"
            };

            var placeholder = new GroupedBarChartPlaceHolder<object, GroupRow>(
                name: "grouped",
                map: _ => rows,
                label: x => x.Category,
                value: x => x.Value,
                seriesName: x => x.Series,
                configuration: config);

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("<svg", svg);
            Assert.Contains("Years", svg);
            Assert.Contains("2024", svg);
            Assert.Contains("2025", svg);
        }

        [Fact]
        public void PieChartPlaceHolder_UsesOverrideCultureForLabels()
        {
            var rows = new[] { new PieRow("A", 1.5) };
            var config = new PieChartConfig { InsideLabelFormat = "{0:0.00}" };
            var placeholder = new PieChartPlaceHolder<object, PieRow>(
                name: "pie",
                map: _ => rows,
                label: x => x.Label,
                value: x => x.Value,
                configuration: config,
                overrideGlobalCultureInfo: new CultureInfo("es-ES"));

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("<svg", svg);
            Assert.Contains("1,50", svg);
        }

        [Fact]
        public void BarChartPlaceHolder_ReturnsNull_WhenMapReturnsNull()
        {
            var placeholder = new BarChartPlaceHolder<object, BarRow>(
                name: "bar",
                map: _ => null!,
                label: x => x.Category,
                value: x => x.Value);

            var result = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void GroupedBarChartPlaceHolder_ReturnsNull_WhenMapReturnsNull()
        {
            var placeholder = new GroupedBarChartPlaceHolder<object, GroupRow>(
                name: "grouped",
                map: _ => null!,
                label: x => x.Category,
                value: x => x.Value,
                seriesName: x => x.Series);

            var result = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void PieChartPlaceHolder_EmptyData_ReturnsSvg()
        {
            var placeholder = new PieChartPlaceHolder<object, PieRow>(
                name: "pie",
                map: _ => Array.Empty<PieRow>(),
                label: x => x.Label,
                value: x => x.Value);

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("<svg", svg);
        }

        [Fact]
        public void BarChartPlaceHolder_LabelFormatApplied()
        {
            var rows = new[] { new BarRow("Jan", 10.12) };
            var config = new BarChartConfig { LabelFormat = "{0:0.0}" };
            var placeholder = new BarChartPlaceHolder<object, BarRow>(
                name: "bar",
                map: _ => rows,
                label: x => x.Category,
                value: x => x.Value,
                configuration: config);

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("10.1", svg);
        }

        [Fact]
        public void GroupedBarChartPlaceHolder_LabelFormatApplied()
        {
            var rows = new[]
            {
                new GroupRow("Jan", "2024", 10.12),
                new GroupRow("Jan", "2025", 12.34)
            };
            var config = new GroupedBarChartConfig { LabelFormat = "{0:0.0}" };
            var placeholder = new GroupedBarChartPlaceHolder<object, GroupRow>(
                name: "grouped",
                map: _ => rows,
                label: x => x.Category,
                value: x => x.Value,
                seriesName: x => x.Series,
                configuration: config);

            var svg = placeholder.ProcessValue(new object(), CultureInfo.InvariantCulture) as string;

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("10.1", svg);
            Assert.Contains("12.3", svg);
        }

        private static string HexToRgb(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex)) throw new ArgumentException("hex is null/empty", nameof(hex));
            hex = hex.Trim();
            if (hex.StartsWith("#")) hex = hex[1..];

            if (hex.Length == 3) // e.g. #f0a
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);

            if (hex.Length != 6) throw new ArgumentException("Expected #RRGGBB or #RGB", nameof(hex));

            var r = int.Parse(hex.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var g = int.Parse(hex.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            var b = int.Parse(hex.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            return $"rgb({r},{g},{b})";
        }
    }
}
