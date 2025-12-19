using OxyPlot;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using zPdfGenerator.Globalization;

namespace zPdfGenerator.Html
{
    internal static class SvgExtensions
    {
        /// <summary>
        /// This method is a workaround to make the SVG responsive by adding a viewBox and removing width/height attributes.
        /// </summary>
        /// <param name="svg">The SVG contents.</param>
        /// <returns>Returns the modified SVG.</returns>
        internal static string MakeResponsive(this string svg)
        {
            if (string.IsNullOrWhiteSpace(svg)) return svg;

            // Find opening tag <svg ...>
            var m = Regex.Match(svg, @"<svg\b[^>]*>", RegexOptions.IgnoreCase);
            if (!m.Success) return svg;

            var svgTag = m.Value;

            // Extracts width/height from <svg>
            var mw = Regex.Match(svgTag, @"\bwidth=""(?<w>\d+(\.\d+)?)""", RegexOptions.IgnoreCase);
            var mh = Regex.Match(svgTag, @"\bheight=""(?<h>\d+(\.\d+)?)""", RegexOptions.IgnoreCase);

            // If there is not width or height, return original SVG
            if (!mw.Success || !mh.Success) return svg;

            var w = mw.Groups["w"].Value;
            var h = mh.Groups["h"].Value;

            // Removes width/height just from the <svg>
            svgTag = Regex.Replace(svgTag, @"\s+\bwidth=""[^""]*""", "", RegexOptions.IgnoreCase);
            svgTag = Regex.Replace(svgTag, @"\s+\bheight=""[^""]*""", "", RegexOptions.IgnoreCase);

            // Adds viewBox if it doesn't exist (just in <svg>)
            if (!Regex.IsMatch(svgTag, @"\bviewBox=""", RegexOptions.IgnoreCase))
            {
                svgTag = svgTag.Replace("<svg", $@"<svg viewBox=""0 0 {w} {h}""", StringComparison.OrdinalIgnoreCase);
            }

            // Replaces the original SVG tag
            return svg.Substring(0, m.Index) + svgTag + svg.Substring(m.Index + m.Length);
        }

        /// <summary>
        /// This method removes any XML headers or declarations before the SVG tag.
        /// </summary>
        /// <param name="svg">The SVG contents</param>
        /// <returns>Returns the modified SVG.</returns>
        internal static string RemoveXmlHeaders(this string svg)
        {
            var idx = svg.IndexOf("<svg", StringComparison.OrdinalIgnoreCase);
            return idx >= 0 ? svg.Substring(idx).Trim() : svg;
        }

        /// <summary>
        /// This method exports the PlotModel to an SVG string.
        /// </summary>
        /// <param name="model">The PlotModel with all the definition for the graphic</param>
        /// <param name="width">The width for the graph. If null provided, we will assume 800 pixels.</param>
        /// <param name="height">The height for the graph. If null provided, we will assume 450 pixels.</param>
        /// <param name="culture">The culture for rendering the SVG.</param>
        /// <returns>Returns an string with the SVG rendered.</returns>
        internal static string ExportPlot(this PlotModel model, int? width, int? height, CultureInfo? culture)
        {
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
                var rendered = Encoding.UTF8.GetString(ms.ToArray());
                return rendered.RemoveXmlHeaders().MakeResponsive();
            }
        }
    }
}
