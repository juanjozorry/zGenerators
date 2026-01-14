using System;
using System.Collections.Generic;
using System.Globalization;
using Fluid;
using zPdfGenerator.Html.FluidFilters;

namespace zPdfGenerator.Tests.Html
{
    public class FluidFiltersTests
    {
        [Fact]
        public void FormatCurrency_UsesCulturePatternAndDecimals()
        {
            var template = "{{ amount | format_currency: '€', 2 }}";
            var output = Render(template, new Dictionary<string, object?>
            {
                ["amount"] = 1234.56m
            }, new CultureInfo("es-ES"));

            Assert.Equal("1.234,56 €", output);
        }

        [Fact]
        public void FormatDate_UsesDefaultFormat_WhenEmpty()
        {
            var template = "{{ d | format_date }}";
            var output = Render(template, new Dictionary<string, object?>
            {
                ["d"] = new DateTime(2025, 1, 10)
            }, new CultureInfo("es-ES"));

            Assert.Equal("10/01/2025", output);
        }

        [Fact]
        public void FormatDateTime_UsesCustomFormat()
        {
            var template = "{{ d | format_datetime: 'dd/MM/yyyy HH:mm' }}";
            var output = Render(template, new Dictionary<string, object?>
            {
                ["d"] = new DateTime(2025, 1, 10, 14, 35, 0)
            }, new CultureInfo("es-ES"));

            Assert.Equal("10/01/2025 14:35", output);
        }

        [Fact]
        public void FormatDatePreset_UsesIso()
        {
            var template = "{{ d | format_date_preset: 'iso' }}";
            var output = Render(template, new Dictionary<string, object?>
            {
                ["d"] = new DateTime(2025, 1, 10)
            }, CultureInfo.InvariantCulture);

            Assert.Equal("2025-01-10", output);
        }

        [Fact]
        public void FormatDate_ReturnsInput_WhenNotDate()
        {
            var template = "{{ d | format_date }}";
            var output = Render(template, new Dictionary<string, object?>
            {
                ["d"] = "not-a-date"
            }, CultureInfo.InvariantCulture);

            Assert.Equal("not-a-date", output);
        }

        private static string Render(string template, IDictionary<string, object?> model, CultureInfo culture)
        {
            var parser = new FluidParser();
            if (!parser.TryParse(template, out var fluidTemplate, out var error))
            {
                throw new InvalidOperationException($"Template parse error: {error}");
            }

            var options = new TemplateOptions
            {
                CultureInfo = culture
            };

            options.Filters.WithBusinessFilters();
            options.Filters.WithDateFilters();

            var context = new TemplateContext(model, options)
            {
                CultureInfo = culture
            };

            return fluidTemplate.Render(context);
        }
    }
}
