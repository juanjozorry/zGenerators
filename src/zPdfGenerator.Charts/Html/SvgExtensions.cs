using System;
using System.Text.RegularExpressions;

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
    }
}
