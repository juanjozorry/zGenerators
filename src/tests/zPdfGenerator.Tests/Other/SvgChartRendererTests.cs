using System.Globalization;
using System.Runtime.CompilerServices;
using zPdfGenerator.Html;

namespace zPdfGenerator.Tests.Other
{
    public sealed class SvgChartRendererTests
    {
        private sealed record PieRow(string Label, double Value);
        private sealed record BarRow(string Category, double Value);
        private sealed record GroupRow(string Category, string Series, double Value);

        [Fact]
        public void GeneratePieSvg_NullItems_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                SvgChartRenderer.GeneratePieSvg<PieRow>(
                    items: null!,
                    label: x => x.Label,
                    value: x => x.Value));
        }

        [Fact]
        public void GeneratePieSvg_NullLabelFunc_Throws()
        {
            var data = new[] { new PieRow("A", 10) };

            Assert.Throws<ArgumentNullException>(() =>
                SvgChartRenderer.GeneratePieSvg(
                    data,
                    label: null!,
                    value: x => x.Value));
        }

        [Fact]
        public void GeneratePieSvg_FiltersEmptyLabels_AndProducesSvg()
        {
            var data = new[]
            {
            new PieRow("A", 10),
            new PieRow("",  20),
            new PieRow("   ", 30),
            new PieRow("B", 40),
        };

            var svg = SvgChartRenderer.GeneratePieSvg(
                data,
                label: x => x.Label,
                value: x => x.Value,
                title: "My Pie",
                legend: "Legend");

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.False(string.IsNullOrWhiteSpace(svg));
            Assert.Contains("<svg", svg);
            Assert.Contains("My Pie", svg);
            // Should include labels A and B but not blanks
            Assert.Contains(">A<", svg);
            Assert.Contains(">B<", svg);
        }

        [Fact]
        public void GeneratePieSvg_AppliesPaletteHex()
        {
            var data = new[]
            {
            new PieRow("A", 10),
            new PieRow("B", 20),
            new PieRow("C", 30),
        };

            var palette = new[] { "#ff0000", "#00ff00", "#0000ff" };

            var svg = SvgChartRenderer.GeneratePieSvg(
                data,
                label: x => x.Label,
                value: x => x.Value,
                paletteHex: palette);

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.Contains("<svg", svg);

            // At least one palette color should appear in exported svg
            var rgb0 = HexToRgb(palette[0]);
            var rgb1 = HexToRgb(palette[1]);
            var rgb2 = HexToRgb(palette[2]);

            Assert.True(svg.Contains(rgb0) || svg.Contains(rgb1) || svg.Contains(rgb2),
                "Expected at least one palette color to appear in the SVG output.");
        }

        [Fact]
        public void GenerateVerticalBarChartSvg_ProducesSvg_WithCategories_AndFillColor()
        {
            var data = new[]
            {
            new BarRow("Jan", 10),
            new BarRow("Feb", 25),
            new BarRow("Mar", 7),
        };

            var fill = "#3366cc";
            var svg = SvgChartRenderer.GenerateBarChartSvg(
                data,
                category: x => x.Category,
                value: x => x.Value,
                chartOrientation: BarChartOrientationEnum.Vertical,
                labelPlacement: LabelPlacementEnum.Inside,
                fillColorHex: fill,
                labelFormat: "{0:0.##}",
                title: "Bar Chart",
                legend: "Revenue");

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.Contains("<svg", svg);
            Assert.Contains("Bar Chart", svg);

            // Category labels appear as text nodes
            Assert.Contains(">Jan<", svg);
            Assert.Contains(">Feb<", svg);
            Assert.Contains(">Mar<", svg);

            // Color appears (often as rgb)
            Assert.Contains(HexToRgb(fill), svg);
        }

        [Fact]
        public void GenerateVerticalGroupedBarChartSvg_ProducesSvg_WithLegendAndPalette()
        {
            var data = new[]
            {
            new GroupRow("Jan", "2024", 10),
            new GroupRow("Jan", "2025", 12),
            new GroupRow("Feb", "2024", 7),
            new GroupRow("Feb", "2025", 15),
        };

            var palette = new[] { "#ff0000", "#00ff00" };

            var svg = SvgChartRenderer.GenerateGroupedBarChartSvg(
                data,
                category: x => x.Category,
                seriesName: x => x.Series,
                value: x => x.Value,
                chartOrientation: BarChartOrientationEnum.Vertical,
                labelPlacement: LabelPlacementEnum.Outside,
                paletteHex: palette,
                labelFormat: "{0:0}",
                title: "Grouped",
                legend: "Years");

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.Contains("<svg", svg);
            Assert.Contains("Grouped", svg);

            // Legend title and series names should be present
            Assert.Contains("Years", svg);
            Assert.Contains("2024", svg);
            Assert.Contains("2025", svg);

            // Palette colors should appear
            Assert.Contains(HexToRgb(palette[0]), svg);
            Assert.Contains(HexToRgb(palette[1]), svg);
        }

        [Fact]
        public void GenerateHorizontalBarChartSvgHorizontal_ProducesSvg_WithCategories_AndFillColor()
        {
            var data = new[]
            {
            new BarRow("Jan", 10),
            new BarRow("Feb", 25),
            new BarRow("Mar", 7),
        };

            var fill = "#3366cc";
            var svg = SvgChartRenderer.GenerateBarChartSvg(
                data,
                category: x => x.Category,
                value: x => x.Value,
                chartOrientation: BarChartOrientationEnum.Horizontal,
                labelPlacement: LabelPlacementEnum.Inside,
                fillColorHex: fill,
                labelFormat: "{0:0.##}",
                title: "Bar Chart",
                legend: "Revenue");

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.Contains("<svg", svg);
            Assert.Contains("Bar Chart", svg);

            // Category labels appear as text nodes
            Assert.Contains(">Jan<", svg);
            Assert.Contains(">Feb<", svg);
            Assert.Contains(">Mar<", svg);

            // Color appears (often as rgb)
            Assert.Contains(HexToRgb(fill), svg);
        }

        [Fact]
        public void GenerateHorizontalGroupedBarChartSvg_ProducesSvg_WithLegendAndPalette()
        {
            var data = new[]
            {
            new GroupRow("Jan", "2024", 10),
            new GroupRow("Jan", "2025", 12),
            new GroupRow("Feb", "2024", 7),
            new GroupRow("Feb", "2025", 15),
        };

            var palette = new[] { "#ff0000", "#00ff00" };

            var svg = SvgChartRenderer.GenerateGroupedBarChartSvg(
                data,
                category: x => x.Category,
                seriesName: x => x.Series,
                value: x => x.Value,
                chartOrientation: BarChartOrientationEnum.Horizontal,
                labelPlacement: LabelPlacementEnum.Outside,
                paletteHex: palette,
                labelFormat: "{0:0}",
                title: "Grouped",
                legend: "Years");

            Assert.NotNull(svg);
            Assert.NotEmpty(svg);
            DumpSvg(svg);

            Assert.Contains("<svg", svg);
            Assert.Contains("Grouped", svg);

            // Legend title and series names should be present
            Assert.Contains("Years", svg);
            Assert.Contains("2024", svg);
            Assert.Contains("2025", svg);

            // Palette colors should appear
            Assert.Contains(HexToRgb(palette[0]), svg);
            Assert.Contains(HexToRgb(palette[1]), svg);
        }

        private static void DumpSvg(string svg, [CallerMemberName] string testName = "")
        {
            File.WriteAllText($"{testName}.svg", svg);
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
