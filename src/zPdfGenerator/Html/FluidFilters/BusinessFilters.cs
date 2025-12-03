using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace zPdfGenerator.Html.FluidFilters
{
    /// <summary>
    /// Provides extension methods for registering business-related filters with a FilterCollection.
    /// </summary>
    /// <remarks>This static class is intended to group filters that perform business-specific formatting or
    /// processing, such as currency formatting. Use the provided extension methods to add these filters to a
    /// FilterCollection instance for use in template rendering scenarios.</remarks>
    public static class BusinessFilters
    {
        /// <summary>
        /// Adds business-related filters, including a currency formatting filter, to the specified filter collection.
        /// </summary>
        /// <remarks>This method extends the provided filter collection by registering additional filters
        /// commonly used in business scenarios. It enables fluent configuration by returning the original
        /// collection.</remarks>
        /// <param name="filters">The filter collection to which business filters will be added. Cannot be null.</param>
        /// <returns>The same <see cref="FilterCollection"/> instance with the business filters added.</returns>
        public static FilterCollection WithBusinessFilters(this FilterCollection filters)
        {
            filters.AddFilter("format_currency", FormatCurrency);
            return filters;
        }

        private static ValueTask<FluidValue> FormatCurrency(
            FluidValue input,
            FilterArguments args,
            TemplateContext context)
        {
            var amount = input.ToNumberValue();

            string currency = args.At(0).ToStringValue();
            int decimals = 2;
            if (args.Count > 1)
                decimals = (int)args.At(1).ToNumberValue();

            var culture = context.CultureInfo ?? CultureInfo.InvariantCulture;

            string formattedNumber = amount.ToString($"N{decimals}", culture);

            int pattern = culture.NumberFormat.CurrencyPositivePattern;

            string result = pattern switch
            {
                0 => currency + formattedNumber,         // €234.000,00
                1 => formattedNumber + currency,         // 234.000,00€
                2 => currency + " " + formattedNumber,   // € 234.000,00   (es-ES habitual)
                3 => formattedNumber + " " + currency,   // 234.000,00 €
                _ => formattedNumber + " " + currency
            };

            return new ValueTask<FluidValue>(new StringValue(result));
        }
    }
}