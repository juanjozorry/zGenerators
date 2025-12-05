using System;
using System.Globalization;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;

namespace zPdfGenerator.Html.FluidFilters
{
    /// <summary>
    /// Provides extension methods for registering date time related filters with a FilterCollection.
    /// </summary>
    /// <remarks>This static class is intended to group filters that perform date time specific formatting or
    /// processing. Use the provided extension methods to add these filters to a
    /// FilterCollection instance for use in template rendering scenarios.</remarks>
    public static class DateFilters
    {
        /// <summary>
        /// Adds date-related formatting filters to the specified filter collection.
        /// </summary>
        /// <remarks>This method adds filters for formatting dates, date-times, and date presets. It is
        /// intended to be used as an extension method to enhance the filter collection with common date formatting
        /// capabilities.</remarks>
        /// <param name="filters">The filter collection to which date formatting filters will be added. Cannot be null.</param>
        /// <returns>The same filter collection instance with date formatting filters included.</returns>
        public static FilterCollection WithDateFilters(this FilterCollection filters)
        {
            filters.AddFilter("format_date", FormatDate);
            filters.AddFilter("format_datetime", FormatDateTime);
            filters.AddFilter("format_date_preset", FormatDatePreset);
            return filters;
        }

        /// <summary>
        /// format_date: formats only the date.
        /// Usage:
        ///   {{ row.date | format_date: "dd/MM/yyyy" }}
        ///   {{ row.date | format_date }}  -> default format "dd/MM/yyyy"
        /// </summary>
        private static ValueTask<FluidValue> FormatDate(
            FluidValue input,
            FilterArguments args,
            TemplateContext context)
        {
            if (!input.TryGetDateTimeInput(context, out var dt))
            {
                return new ValueTask<FluidValue>(new StringValue(input.ToStringValue()));
            }

            var format = args.At(0).ToStringValue();
            if (string.IsNullOrEmpty(format))
                format = "dd/MM/yyyy";

            var culture = context.CultureInfo ?? CultureInfo.InvariantCulture;
            var formatted = dt.ToString(format, culture);

            return new ValueTask<FluidValue>(new StringValue(formatted));
        }

        /// <summary>
        /// format_datetime: fecha + hora.
        /// Usage:
        ///   {{ row.date | format_datetime: "dd/MM/yyyy HH:mm" }}
        ///   {{ row.date | format_datetime }}  -> "dd/MM/yyyy HH:mm"
        /// </summary>
        private static ValueTask<FluidValue> FormatDateTime(
            FluidValue input,
            FilterArguments args,
            TemplateContext context)
        {
            if (!input.TryGetDateTimeInput(context, out var dt))
            {
                return new ValueTask<FluidValue>(new StringValue(input.ToStringValue()));
            }

            var format = args.At(0).ToStringValue();
            if (string.IsNullOrEmpty(format))
                format = "dd/MM/yyyy HH:mm";

            var culture = context.CultureInfo ?? CultureInfo.InvariantCulture;
            var formatted = dt.ToString(format, culture);

            return new ValueTask<FluidValue>(new StringValue(formatted));
        }

        /// <summary>
        /// format_date_preset: allows using names like "short", "long", etc.
        /// Usage:
        ///   {{ row.date | format_date_preset: "short" }}
        ///   {{ row.date | format_date_preset: "long" }}
        ///   {{ row.date | format_date_preset: "monthYear" }}
        /// </summary>
        private static ValueTask<FluidValue> FormatDatePreset(
            FluidValue input,
            FilterArguments args,
            TemplateContext context)
        {
            if (!input.TryGetDateTimeInput(context, out var dt))
            {
                return new ValueTask<FluidValue>(new StringValue(input.ToStringValue()));
            }

            var presetName = args.At(0).ToStringValue();
            if (string.IsNullOrEmpty(presetName))
                presetName = "short";

            var culture = context.CultureInfo ?? CultureInfo.InvariantCulture;

            // Small switch for typical presets
            string format = presetName.ToLowerInvariant() switch
            {
                "short" => culture.DateTimeFormat.ShortDatePattern,     // 10/01/2025
                "long" => culture.DateTimeFormat.LongDatePattern,       // viernes, 10 de enero de 2025
                "shorttime" => culture.DateTimeFormat.ShortTimePattern, // 14:35
                "longtime" => culture.DateTimeFormat.LongTimePattern,   // 14:35:00
                "full" => culture.DateTimeFormat.FullDateTimePattern,   // date + time long
                "monthyear" => "MMMM yyyy",                             // enero 2025
                "iso" => "yyyy-MM-dd",                                  // ISO simple
                _ => "dd/MM/yyyy"                                       // fallback
            };

            var formatted = dt.ToString(format, culture);
            return new ValueTask<FluidValue>(new StringValue(formatted));
        }
    }
}
