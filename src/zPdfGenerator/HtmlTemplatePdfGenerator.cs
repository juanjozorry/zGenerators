using HtmlAgilityPack;
using iText.License;
using Microsoft.Extensions.Logging;
using zPdfGenerator.HtmlPlaceHolders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace zPdfGenerator
{
    /// <summary>
    /// This is a generic class used to generate a PDF from a html template and data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public abstract class HtmlTemplatePdfGenerator<T>
    {
        private readonly string basePath;
        private readonly string templateFile;

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlTemplatePdfGenerator{T}" /> class.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="licenseFile">The license file.</param>
        /// <param name="logger">The logger.</param>
        public HtmlTemplatePdfGenerator(string templateFile, string basePath, string licenseFile, ILogger<HtmlTemplatePdfGenerator<T>> logger)
        {
            this.templateFile = templateFile;
            this.Logger = logger;
            this.basePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), basePath);
            bool useLicense = false;
            if (licenseFile != null)
            {
                var licenseFilePath = Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), licenseFile);
                if (File.Exists(licenseFilePath))
                {
                    useLicense = true;
                    LicenseKey.LoadLicenseFile(licenseFilePath);
                }
            }
            
            if (!useLicense)
            {
                Logger.LogWarning("No license file found or wrong path. Using itext AGPL mode.");
            }

        }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public CultureInfo Culture { get; set; } = CultureInfo.GetCultureInfo("es-ES");

        /// <summary>
        /// Gets the place holders.
        /// </summary>
        /// <value>The place holders.</value>
        public abstract IEnumerable<BasePlaceHolder<T>> PlaceHolders { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger<HtmlTemplatePdfGenerator<T>> Logger { get; private set; }

        /// <summary>
        /// Generates the PDF.
        /// </summary>
        /// <param name="dataPopulation">The data population.</param>
        /// <param name="cancellationToken">The cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>System.Byte[].</returns>
        protected async Task<byte[]> GeneratePDF(Func<Task<T>> dataPopulation, CancellationToken cancellationToken = default)
        {
            var htmlContents = await PopulateTemplate(dataPopulation);

            // Resulting HTML is only traced in debug mode to avoid private data in server logs (just development purposes)
            Logger.LogDebug(htmlContents);

            var sw = Stopwatch.StartNew();

            Logger.LogInformation($"About to generate a PDF from html template of size: {htmlContents?.Length} bytes");

            var pdf = await PdfHelpers.ConvertHtmlToPDF(htmlContents, basePath, cancellationToken);

            Logger.LogInformation($"Generated PDF, took {sw.ElapsedMilliseconds}ms");

            return pdf;
        }

        /// <summary>
        /// Populates the template with data.
        /// </summary>
        /// <typeparam name="TItem">The type of the t item.</typeparam>
        /// <param name="placeHolders">The items.</param>
        /// <param name="item">The data.</param>
        /// <param name="htmlNode">The root node.</param>
        protected void PopulateTemplateWithData<TItem>(IEnumerable<BasePlaceHolder<TItem>> placeHolders, TItem item, HtmlNode htmlNode)
        {
            if (placeHolders is null) throw new NullReferenceException($"{nameof(placeHolders)} parameter is mandatory");
            if (item is null) throw new NullReferenceException($"{nameof(item)} parameter is mandatory");
            if (htmlNode is null) throw new NullReferenceException($"{nameof(htmlNode)} parameter is mandatory");

            foreach (var placeHolder in placeHolders)
            {
                try
                {
                    placeHolder.ProcessNode(htmlNode, item, this.Culture, Logger);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Error trying to replace place holder {placeHolder.Name}: {ex}");
                }
            }
        }

        /// <summary>
        /// Loads the template.
        /// </summary>
        private HtmlDocument LoadTemplate()
        {
            var templateDocument = new HtmlDocument();
            var path = Path.Combine(basePath, this.templateFile);
            Logger.LogInformation($"About to load template from {path}");
            templateDocument.Load(path);

            return templateDocument;
        }

        private async Task<string> PopulateTemplate(Func<Task<T>> dataPopulation)
        {
            HtmlDocument templateDocument = null;
            try
            {
                var threadCulture = CultureInfo.CurrentCulture;
                var threadUICulture = CultureInfo.CurrentUICulture;

                CultureInfo.CurrentCulture = this.Culture;
                CultureInfo.CurrentUICulture = this.Culture;

                var data = await dataPopulation.Invoke();

                if (data is null) throw new NullReferenceException($"{nameof(data)} parameter is mandatory");

                Logger.LogInformation($"Starting the population for the template {templateFile}");

                var sw = Stopwatch.StartNew();

                templateDocument = LoadTemplate();

                PopulateTemplateWithData(PlaceHolders, data, templateDocument.DocumentNode);

                CultureInfo.CurrentCulture = threadCulture;
                CultureInfo.CurrentUICulture = threadUICulture;

                Logger.LogInformation($"Populated template took {sw.ElapsedMilliseconds}ms");

                return templateDocument.DocumentNode.InnerHtml;
            }
            finally
            {
                if (templateDocument != null) templateDocument = null;
            }
        }
    }
}
