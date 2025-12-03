using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using iText.License;
using Microsoft.Extensions.Logging;
using zPdfGenerator.FormPlaceHolders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace zPdfGenerator
{
    /// <summary>
    /// Class FormPdfGenerator.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class FormPdfGenerator<T>
    {
        private readonly string basePath;
        private readonly string templateFile;
        /// <summary>
        /// Initializes a new instance of the <see cref="FormPdfGenerator{T}"/> class.
        /// </summary>
        /// <param name="templateFile">The template file.</param>
        /// <param name="basePath">The base path.</param>
        /// <param name="licenseFile">The license file.</param>
        /// <param name="logger">The logger.</param>
        public FormPdfGenerator(string templateFile, string basePath, string licenseFile, ILogger<FormPdfGenerator<T>> logger)
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
        /// Gets the form elements to remove.
        /// </summary>
        /// <value>The form elements to remove.</value>
        public virtual IEnumerable<string> FormElementsToRemove { get; }

        /// <summary>
        /// Gets the place holders.
        /// </summary>
        /// <value>The place holders.</value>
        public abstract IEnumerable<BasePlaceHolder<T>> PlaceHolders { get; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        /// <value>The logger.</value>
        protected ILogger<FormPdfGenerator<T>> Logger { get; private set; }

        /// <summary>
        /// Generates the PDF.
        /// </summary>
        /// <param name="dataPopulation">The data population.</param>
        /// <returns>System.Byte[].</returns>
        protected async Task<byte[]> GeneratePDF(Func<Task<T>> dataPopulation)
        {
            Logger.LogInformation($"Getting data for the template {templateFile}");
            var threadCulture = CultureInfo.CurrentCulture;
            var threadUICulture = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentCulture = this.Culture;
            CultureInfo.CurrentUICulture = this.Culture;
            var data = await dataPopulation.Invoke();

            if (data is null) throw new NullReferenceException($"{nameof(data)} parameter is mandatory");

            Logger.LogInformation($"Starting the population for the template {templateFile}");

            var sw = Stopwatch.StartNew();

            using (var stream = new MemoryStream())
            {
                var path = Path.Combine(basePath, this.templateFile);
                Logger.LogInformation($"About to load template from {path}");

                using (var pdf = new PdfDocument(new PdfReader(path), new PdfWriter(stream)))
                {
                    PopulateFormWithData(data, pdf);

                    Logger.LogInformation($"Generated PDF, took {sw.ElapsedMilliseconds}ms");
                    pdf.Close();

                    return stream.ToArray();
                }
            }
        }

        private void PopulateFormWithData(T data, PdfDocument pdf)
        {
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
            IDictionary<string, PdfFormField> pdfFields = form.GetAllFormFields();

            foreach (var element in FormElementsToRemove ?? Enumerable.Empty<string>())
            {
                if (pdfFields.ContainsKey(element)) form.RemoveField(element);
                else Logger.LogWarning($"The form does not contain a key with name {element} to be removed, skipping");
            }

            foreach (var placeHolder in PlaceHolders)
            {
                if (pdfFields.ContainsKey(placeHolder.Name))
                {
                    try
                    {
                        var val = placeHolder.ProcessData(data, this.Culture);
                        pdfFields[placeHolder.Name].SetValue(val == null ? string.Empty : val, true);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarning($"Error trying to populate place holder {placeHolder.Name}: {ex}");
                    }
                }
                else
                {
                    Logger.LogWarning($"The form does not contain a key with name {placeHolder.Name} to be populated, skipping");
                }
            }

            form.FlattenFields();
        }
    }
}
