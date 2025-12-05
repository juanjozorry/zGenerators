using Fluid;
using Fluid.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using zPdfGenerator.Html.FluidFilters;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Defines a service for generating PDF documents from HTML templates using Fluid and a configurable builder.
    /// </summary>
    /// <remarks>This interface enables the creation of PDF files by rendering HTML templates with Fluid and
    /// applying custom configuration through a builder. Implementations may support additional options such as template
    /// data binding, styling, and output customization. Thread safety and performance characteristics depend on the
    /// specific implementation.</remarks>
    public interface IFluidHtmlTemplatePdfGenerator
    {
        /// <summary>
        /// Generates a PDF document from the specified model using the provided configuration action.
        /// </summary>
        /// <typeparam name="T">The type of the model to be rendered in the PDF document.</typeparam>
        /// <param name="configure">An action that configures the PDF generation process for the specified model type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. Returns an empty array if the generation fails.</returns>
        byte[] GeneratePdf<T>(Action<FluidHtmlPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides functionality for generating PDF documents from HTML templates using the Fluid templating engine.
    /// </summary>
    /// <remarks>Use this class to render HTML templates with dynamic data and convert the resulting content
    /// into PDF format. This is useful for scenarios such as generating reports, invoices, or other documents where
    /// template-based PDF output is required.</remarks>
    public class FluidHtmlTemplatePdfGenerator : IFluidHtmlTemplatePdfGenerator
    {
        private readonly ILogger<FluidHtmlTemplatePdfGenerator> _logger;
        private readonly IHtmlToPdfConverter _htmlToPdfConverter;

        /// <summary>
        /// Initializes a new instance of the FluidHtmlTemplatePdfGenerator class.
        /// </summary>
        /// <param name="logger">The logger instance used to record diagnostic and operational information for PDF generation activities.
        /// Cannot be null.</param>
        /// <param name="htmlToPdfConverter">Converts HTML to PDF</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public FluidHtmlTemplatePdfGenerator(ILogger<FluidHtmlTemplatePdfGenerator> logger, IHtmlToPdfConverter htmlToPdfConverter)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._htmlToPdfConverter = htmlToPdfConverter ?? throw new ArgumentNullException(nameof(htmlToPdfConverter));
        }

        /// <summary>
        /// Generates a PDF document from the specified model using the provided configuration action.
        /// </summary>
        /// <typeparam name="T">The type of the model to be rendered in the PDF document.</typeparam>
        /// <param name="configure">An action that configures the PDF generation process for the specified model type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. Returns an empty array if the generation fails.</returns>
        public byte[] GeneratePdf<T>(Action<FluidHtmlPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            cancellationToken.ThrowIfCancellationRequested();

            var builder = new FluidHtmlPdfGeneratorBuilder<T>();
            configure(builder);

            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(builder.TemplatePath))
                throw new InvalidOperationException("TemplatePath must be configured in FormPdfGeneratorBuilder.");

            if (builder.DataItem == null)
                throw new InvalidOperationException("DataItem must be set up in FormPdfGeneratorBuilder.");

            if (builder.PlaceHolders == null)
                throw new InvalidOperationException("PlaceHolders must be set up in FormPdfGeneratorBuilder.");

            _logger.LogInformation("Starting PDF form generation using template {TemplatePath}.", builder.TemplatePath);

            if (!PdfHelpers.LoadLicenseFile(builder.LicensePath))
            {
                _logger.LogWarning("No license file found or wrong path. Using itext AGPL mode.");
            }
            cancellationToken.ThrowIfCancellationRequested();

            var threadCulture = CultureInfo.CurrentCulture;
            var threadUICulture = CultureInfo.CurrentUICulture;

            try
            {

                CultureInfo.CurrentCulture = builder.CultureInfo;
                CultureInfo.CurrentUICulture = builder.CultureInfo;

                _logger.LogInformation("Starting the population for the template {TemplatePath)}", Path.GetFileName(builder.TemplatePath));

                var sw = Stopwatch.StartNew();

                _logger.LogInformation("About to load template from {TemplatePath} with culture {Culture}", builder.TemplatePath, builder.CultureInfo.Name);

                var renderedTemplate = RenderTemplate(builder, cancellationToken);

                _logger.LogDebug("Template after {Template}", renderedTemplate);

                _logger.LogInformation("Finished the population for the template {TemplatePath} in {ElapsedMilliseconds} ms", Path.GetFileName(builder.TemplatePath), sw.ElapsedMilliseconds);

                if (renderedTemplate is null)
                    throw new InvalidOperationException("Rendered template cannot be null");

                cancellationToken.ThrowIfCancellationRequested();

                var basePath = Path.GetDirectoryName(builder.TemplatePath) ?? AppContext.BaseDirectory;
                var pdf = _htmlToPdfConverter.ConvertHtmlToPDF(renderedTemplate, basePath, cancellationToken);

                _logger.LogInformation("Finished the conversion to PDF in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                return pdf;
            }
            finally
            {
                CultureInfo.CurrentCulture = threadCulture;
                CultureInfo.CurrentUICulture = threadUICulture;
            }
        }

        private string? RenderTemplate<T>(FluidHtmlPdfGeneratorBuilder<T> builder, CancellationToken cancellationToken)
        {
            var templateText = File.ReadAllText(builder.TemplatePath);
            var parser = new FluidParser();

            if (!parser.TryParse(templateText, out var template, out var error))
            {
                throw new InvalidOperationException($"Error parsing template '{builder.TemplatePath}': {error}");
            }

            var options = new TemplateOptions
            {
                CultureInfo = builder.CultureInfo
            };

            // Register custom filters
            options.Filters.WithNumberFilters();
            options.Filters.WithBusinessFilters();
            options.Filters.WithDateFilters();

            options.MemberAccessStrategy.Register<T>();
            var context = new TemplateContext(options)
            {
                CultureInfo = options.CultureInfo
            };

            context.SetValue("Model", builder.DataItem!);

            foreach (var ph in builder.PlaceHolders)
            {
                var value = ph.ProcessValue(builder.DataItem!, options.CultureInfo);

                // We need to register the types in case of collections so that Fluid can access their members
                if (value is System.Collections.IEnumerable enumerable && value is not string)
                {
                    foreach (var item in enumerable)
                    {
                        if (item == null) continue;
                        var itemType = item.GetType();
                        options.MemberAccessStrategy.Register(itemType);
                        break;
                    }
                }
                else if (value is not null)
                {
                    var type = value.GetType();
                    if (!type.IsPrimitive && type != typeof(string))
                    {
                        options.MemberAccessStrategy.Register(type);
                    }
                }
                context.SetValue(ph.Name, value);
            }

            cancellationToken.ThrowIfCancellationRequested();

            return template.Render(context);
        }
    }
}
