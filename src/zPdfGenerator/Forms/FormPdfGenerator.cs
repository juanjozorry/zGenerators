using iText.Forms;
using iText.Forms.Fields;
using iText.Kernel.Pdf;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using zPdfGenerator.Globalization;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Forms
{
    /// <summary>
    /// Provides interface for generating PDF documents from a form template.
    /// </summary>
    public interface IFormPdfGenerator
    {
        /// <summary>
        /// Generates a PDF document by populating a template with data and placeholders configured via the specified
        /// builder.
        /// </summary>
        /// <remarks>The method uses the template path and data provided in the builder to generate a PDF
        /// form. If a valid license file is specified and found, it is used; otherwise, the PDF is generated in AGPL
        /// mode. The current thread's culture is temporarily set to the builder's culture during generation. The
        /// operation can be cancelled via the provided cancellation token.</remarks>
        /// <typeparam name="T">The type of the data item to be used for populating the PDF form.</typeparam>
        /// <param name="configure">An action that configures the <see cref="FormPdfGeneratorBuilder{T}"/> with template path, data item,
        /// placeholders, and other options required for PDF generation. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. The array will be empty if the PDF generation fails
        /// before writing any content.</returns>
        byte[] GeneratePdf<T>(Action<FormPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Provides functionality for generating PDF documents from a form template.
    /// </summary>
    public class FormPdfGenerator : IFormPdfGenerator
    {
        private readonly ILogger<FormPdfGenerator> _logger;

        /// <summary>
        /// Initializes a new instance of the FormPdfGenerator class.
        /// </summary>
        /// <param name="logger">The logger instance used to record diagnostic and operational information for PDF generation activities.
        /// Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="logger"/> is null.</exception>
        public FormPdfGenerator(ILogger<FormPdfGenerator> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates a PDF document by populating a template with data and placeholders configured via the specified
        /// builder.
        /// </summary>
        /// <remarks>The method uses the template path and data provided in the builder to generate a PDF
        /// form. If a valid license file is specified and found, it is used; otherwise, the PDF is generated in AGPL
        /// mode. The current thread's culture is temporarily set to the builder's culture during generation. The
        /// operation can be cancelled via the provided cancellation token.</remarks>
        /// <typeparam name="T">The type of the data item to be used for populating the PDF form.</typeparam>
        /// <param name="configure">An action that configures the <see cref="FormPdfGeneratorBuilder{T}"/> with template path, data item,
        /// placeholders, and other options required for PDF generation. Cannot be null.</param>
        /// <param name="cancellationToken">A cancellation token that can be used to cancel the PDF generation operation.</param>
        /// <returns>A byte array containing the generated PDF document. The array will be empty if the PDF generation fails
        /// before writing any content.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="configure"/> is null.</exception>
        /// <exception cref="InvalidOperationException">Thrown if required properties such as TemplatePath, DataItem, or PlaceHolders are not configured in <see
        /// cref="FormPdfGeneratorBuilder{T}"/>.</exception>
        public byte[] GeneratePdf<T>(Action<FormPdfGeneratorBuilder<T>> configure, CancellationToken cancellationToken = default)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            cancellationToken.ThrowIfCancellationRequested();

            var builder = new FormPdfGeneratorBuilder<T>();
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

                using (var stream = new MemoryStream())
                {
                    _logger.LogInformation("About to load template from {TemplatePath}", builder.TemplatePath);

                    using (var pdfDocument = new PdfDocument(new PdfReader(builder.TemplatePath), new PdfWriter(stream)))
                    {
                        PopulateFormWithData(builder, pdfDocument, cancellationToken);

                        _logger.LogInformation("Generated PDF, took {ElapsedMilliseconds}ms", sw.ElapsedMilliseconds);
                        pdfDocument.Close();

                        _logger.LogInformation($"Finished PDF form generation.");
                        var pdf = stream.ToArray();

                        pdf = PostProcessorsHelper.RunPostProcessors(pdf, builder.PostProcessors, cancellationToken);

                        _logger.LogInformation("Finished the PDF generation in {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);

                        return pdf;
                    }
                }
            }
        }

        private void PopulateFormWithData<T>(FormPdfGeneratorBuilder<T> builder, PdfDocument pdf, CancellationToken cancellationToken)
        {
            PdfAcroForm form = PdfAcroForm.GetAcroForm(pdf, true);
            IDictionary<string, PdfFormField> pdfFields = form.GetAllFormFields();

            _logger.LogDebug("PDF Fields: {PdfFields}", string.Join(", ", pdfFields.Keys.Select(k => $"'{k}' -> {pdfFields[k].GetDefaultValue()}")));

            foreach (var element in builder.FormElementsToRemove ?? Enumerable.Empty<string>())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pdfFields.ContainsKey(element)) form.RemoveField(element);
                else _logger.LogWarning("The form does not contain a key with name {element} to be removed, skipping", element);
            }

            foreach (var placeHolder in builder.PlaceHolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (pdfFields.ContainsKey(placeHolder.Name))
                {
                    try
                    {
                        var val = placeHolder.ProcessData(builder.DataItem!, builder.CultureInfo);
                        pdfFields[placeHolder.Name].SetValue(val == null ? string.Empty : val, true);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning("Error trying to populate place holder {Name}: {ex}", placeHolder.Name, ex);
                    }
                }
                else
                {
                    _logger.LogWarning("The form does not contain a key with name {Name} to be populated, skipping", placeHolder.Name);
                }
            }

            if (builder.FlattenFields)
            {
                cancellationToken.ThrowIfCancellationRequested();

                form.FlattenFields();
            }
        }
    }
}