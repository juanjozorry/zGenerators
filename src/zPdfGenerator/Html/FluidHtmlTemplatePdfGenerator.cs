using Fluid;
using Fluid.Filters;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using zPdfGenerator.Globalization;
using zPdfGenerator.Html.FluidFilters;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;
using zPdfGenerator.Html.Helpers;

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

        /// <summary>
        /// Generates a PDF document from the specified model using the provided configuration action.
        /// </summary>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="licensePath">The path to the license file</param>
        /// <param name="model">The model used to populate the template</param>
        /// <param name="culture">The culture</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. Returns an empty array if the generation fails.</returns>
        byte[] GeneratePdf(string templatePath, string? licensePath, IDictionary<string, object?> model, CultureInfo culture, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided data, placeholders, and culture.
        /// </summary>
        /// <typeparam name="T">The type of the model to be rendered in the PDF document.</typeparam>
        /// <param name="configure">An action that configures the PDF generation process for the specified model type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns></returns>
        string RenderHtml<T>(Action<FluidHtmlPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default);

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided data, placeholders, and culture.
        /// </summary>
        /// <typeparam name="T">The type for the data</typeparam>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="data">The data</param>
        /// <param name="placeholders">The placeholders collection</param>
        /// <param name="culture">The culture to use</param>
        /// <param name="ct">A cancellation token</param>
        /// <returns></returns>
        string RenderHtml<T>(string templatePath, T data, IEnumerable<BasePlaceHolder<T>> placeholders,
            CultureInfo culture, CancellationToken ct = default);

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided model and culture.
        /// </summary>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="model">The model used to populate the template</param>
        /// <param name="culture">The culture</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        string RenderHtml(string templatePath, IDictionary<string, object?> model, CultureInfo culture, CancellationToken cancellationToken = default);
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

            using (CultureScope.Use(builder.CultureInfo))
            {
                _logger.LogInformation("Starting the population for the template {TemplatePath)}", Path.GetFileName(builder.TemplatePath));

                var sw = Stopwatch.StartNew();

                _logger.LogInformation("About to load template from {TemplatePath} with culture {Culture}", builder.TemplatePath, builder.CultureInfo.Name);

                var renderedTemplate = RenderHtml(builder.TemplatePath, builder.DataItem, builder.PlaceHolders, builder.CultureInfo, cancellationToken);

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
        }

        /// <summary>
        /// Generates a PDF document from the specified model using the provided configuration action.
        /// </summary>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="licensePath">The path to the license file</param>
        /// <param name="model">The model used to populate the template</param>
        /// <param name="culture">The culture</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. Returns an empty array if the generation fails.</returns>
        public byte[] GeneratePdf(string templatePath, string? licensePath, IDictionary<string, object?> model, CultureInfo culture, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(templatePath))
                throw new InvalidOperationException("TemplatePath must be configured.");

            if (model is null)
                throw new InvalidOperationException("Model must be set up.");

            _logger.LogInformation("Starting PDF form generation using template {TemplatePath}.", templatePath);

            if (!PdfHelpers.LoadLicenseFile(licensePath))
            {
                _logger.LogWarning("No license file found or wrong path. Using itext AGPL mode.");
            }
            cancellationToken.ThrowIfCancellationRequested();

            using (CultureScope.Use(culture))
            {
                _logger.LogInformation("Starting the population for the template {TemplatePath)}", Path.GetFileName(templatePath));

                var sw = Stopwatch.StartNew();

                _logger.LogInformation("About to load template from {TemplatePath} with culture {Culture}", templatePath, culture.Name);

                var renderedTemplate = RenderHtml(templatePath, model, culture, cancellationToken);

                _logger.LogDebug("Template after {Template}", renderedTemplate);

                _logger.LogInformation("Finished the population for the template {TemplatePath} in {ElapsedMilliseconds} ms", Path.GetFileName(templatePath), sw.ElapsedMilliseconds);

                if (renderedTemplate is null)
                    throw new InvalidOperationException("Rendered template cannot be null");

                cancellationToken.ThrowIfCancellationRequested();

                var basePath = Path.GetDirectoryName(templatePath) ?? AppContext.BaseDirectory;
                var pdf = _htmlToPdfConverter.ConvertHtmlToPDF(renderedTemplate, basePath, cancellationToken);

                _logger.LogInformation("Finished the conversion to PDF in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                return pdf;
            }
        }

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided data, placeholders, and culture.
        /// </summary>
        /// <typeparam name="T">The type of the model to be rendered in the PDF document.</typeparam>
        /// <param name="configure">An action that configures the PDF generation process for the specified model type.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. Returns an empty array if the generation fails.</returns>
        public string RenderHtml<T>(Action<FluidHtmlPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default)
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

            var model = BuildModel(builder.DataItem, builder.PlaceHolders, builder.CultureInfo);
            return RenderHtml(builder.TemplatePath, model, builder.CultureInfo, cancellationToken);
        }

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided data, placeholders, and culture.
        /// </summary>
        /// <typeparam name="T">The type for the data</typeparam>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="data">The data</param>
        /// <param name="placeholders">The placeholders collection</param>
        /// <param name="culture">TE culture to use</param>
        /// <param name="ct">A cancellation token</param>
        /// <returns></returns>
        public string RenderHtml<T>(string templatePath, T data, IEnumerable<BasePlaceHolder<T>> placeholders, CultureInfo culture, CancellationToken ct = default)
        {
            var model = BuildModel(data, placeholders, culture);
            return RenderHtml(templatePath, model, culture, ct);
        }

        /// <summary>
        /// Renders an HTML template located at the specified path using the provided model and culture.
        /// </summary>
        /// <param name="templatePath">The path to the template</param>
        /// <param name="model">The model used to populate the template</param>
        /// <param name="culture">The culture</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public string RenderHtml(string templatePath, IDictionary<string, object?> model, CultureInfo culture, CancellationToken cancellationToken = default)
        {
            var templateText = File.ReadAllText(templatePath);
            var parser = new FluidParser();

            if (!parser.TryParse(templateText, out var template, out var error))
            {
                throw new InvalidOperationException($"Error parsing template '{templatePath}': {error}");
            }

            var options = new TemplateOptions
            {
                CultureInfo = culture
            };

            // Register custom filters
            options.Filters.WithNumberFilters();
            options.Filters.WithBusinessFilters();
            options.Filters.WithDateFilters();

            // Registers collections and lists for object graph
            options.MemberAccessStrategy.Register<IDictionary<string, object?>>();
            options.MemberAccessStrategy.Register<IReadOnlyDictionary<string, object?>>();
            options.MemberAccessStrategy.Register<List<object?>>();

            // Dynamic registration
            FluidModelRegistration.RegisterModelTypes(options, model);

            // Register the main model type
            var context = new TemplateContext(model, options)
            {
                CultureInfo = options.CultureInfo
            };

            context.SetValue("Model", model);

            cancellationToken.ThrowIfCancellationRequested();

            return template.Render(context);
        }

        private static Dictionary<string, object?> BuildModel<T>(T data, IEnumerable<BasePlaceHolder<T>> placeholders, CultureInfo culture)
        {
            var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

            foreach (var ph in placeholders)
            {
                if (dict.ContainsKey(ph.Name))
                {
                    throw new InvalidOperationException($"Duplicate placeholder name '{ph.Name}'. Placeholder names must be unique.");
                }

                dict[ph.Name] = ph.ProcessValue(data, culture);
            }

            dict["Model"] = data;

            return dict;
        }
    }
}
