using System;
using System.Collections.Generic;
using System.Globalization;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Provides a builder for configuring and creating instances of a PDF generator from a Fluid HTML template.
    /// </summary>
    /// <remarks>Use this class to fluently configure options and dependencies required to generate PDF
    /// documents from a HTML template. The builder pattern enables step-by-step customization before constructing the final
    /// PDF generator instance.</remarks>
    public static class FluidHtmlPdfGeneratorBuilderExtensions
    {
        /// <summary>
        /// Adds a text placeholder to the PDF form using the specified name and value mapping function.
        /// </summary>
        /// <remarks>If a placeholder with the same name already exists, it will be added again; duplicate
        /// names may result in multiple placeholders in the output. This method is typically used in a fluent
        /// configuration sequence to define multiple placeholders before generating the PDF.</remarks>
        /// <param name="builder">A reference to the builder.</param>
        /// <param name="name">The name of the text placeholder to be added. This value is used as the identifier for the placeholder in
        /// the generated PDF form. Cannot be null or empty.</param>
        /// <param name="map">A function that maps an instance of <typeparamref name="TBase"/> to the string value to be inserted for this
        /// placeholder. Cannot be null.</param>
        /// <param name="label">The function to extract the label for each pie chart segment.</param>
        /// <param name="value">The function to extract the value for each pie chart segment.</param>
        /// <param name="configuration">The configuration of the pie chart.</param>
        /// <param name="overrideGlobalCultureInfo">The culture if the global culture info needs to be overriden.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{TBase}"/> instance, enabling method chaining.</returns>
        public static FluidHtmlPdfGeneratorBuilder<TBase> AddPieChart<TBase, TItem>(this FluidHtmlPdfGeneratorBuilder<TBase> builder, string name, Func<TBase, IEnumerable<TItem>> map, 
            Func<TItem, string> label, Func<TItem, double> value, PieChartConfig configuration, CultureInfo? overrideGlobalCultureInfo = null)
        {
            builder.AddPlaceHolder(new PieChartPlaceHolder<TBase, TItem>(name, map, label, value, configuration, overrideGlobalCultureInfo));
            return builder;
        }
    }
}
