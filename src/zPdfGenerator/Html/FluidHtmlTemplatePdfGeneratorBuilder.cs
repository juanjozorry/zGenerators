using System;
using System.Collections.Generic;
using System.Globalization;
using zPdfGenerator.Forms;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;
using zPdfGenerator.PostProcessors;

namespace zPdfGenerator.Html
{
    /// <summary>
    /// Provides a builder for configuring and creating instances of a PDF generator from a Fluid HTML template.
    /// </summary>
    /// <remarks>Use this class to fluently configure options and dependencies required to generate PDF
    /// documents from a HTML template. The builder pattern enables step-by-step customization before constructing the final
    /// PDF generator instance.</remarks>
    public class FluidHtmlPdfGeneratorBuilder<T>
    {
        /// <summary>
        /// The data item used to resolve all placeholders.
        /// </summary>
        public T? DataItem { get; private set; }

        /// <summary>
        /// Gets the file system path to the license file used by the application.
        /// </summary>
        public string LicensePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the file system path to the template used for rendering or processing operations.
        /// </summary>
        public string TemplatePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the culture information used for formatting and parsing operations.
        /// </summary>
        /// <remarks>This property determines how culture-sensitive data, such as dates and numbers, are
        /// interpreted and displayed. By default, it is set to <see cref="CultureInfo.InvariantCulture"/>. Changing
        /// this property affects all operations that rely on culture-specific formatting within the containing
        /// class.</remarks>
        public CultureInfo CultureInfo { get; private set; } = CultureInfo.InvariantCulture;

        /// <summary>
        /// Gets the collection of placeholders associated with the current instance.
        /// </summary>
        internal List<BasePlaceHolder<T>> PlaceHolders { get; } = new();

        /// <summary>
        /// Gets the collection of post-processors applied after the main processing step.
        /// </summary>
        internal List<IPostProcessor> PostProcessors { get; } = new();

        /// <summary>
        /// Gets the generation options used to control logging and resource access.
        /// </summary>
        public FluidHtmlPdfGenerationOptions GenerationOptions { get; } = new();

        /// <summary>
        /// Assign the data item on which placeholders will be processed.
        /// </summary>
        public FluidHtmlPdfGeneratorBuilder<T> SetData(T data)
        {
            DataItem = data;
            return this;
        }

        /// <summary>
        /// Specifies the path to the license file to be used for PDF generation.
        /// </summary>
        /// <param name="licensePath">The full file system path to the license file. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance with the license file path configured.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="licensePath"/> is null, empty, or consists only of white-space characters.</exception>
        public FluidHtmlPdfGeneratorBuilder<T> UseLicenseFile(string licensePath)
        {
            if (string.IsNullOrWhiteSpace(licensePath)) throw new ArgumentException("License path cannot be null or empty.", nameof(licensePath));

            LicensePath = licensePath;

            return this;
        }

        /// <summary>
        /// Specifies the file system path to the PDF template to be used for form generation.
        /// </summary>
        /// <param name="fullTemplatePath">The full file system path to the PDF template file. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance with the template path configured.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fullTemplatePath"/> is null, empty, or consists only of white-space characters.</exception>
        public FluidHtmlPdfGeneratorBuilder<T> UseTemplatePath(string fullTemplatePath)
        {
            if (string.IsNullOrWhiteSpace(fullTemplatePath))
                throw new ArgumentException("Template path cannot be null or empty.", nameof(fullTemplatePath));

            TemplatePath = fullTemplatePath;
            return this;
        }

        /// <summary>
        /// Sets the culture to be used for formatting and localization operations in the PDF generation process.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use for formatting and localization. If <paramref name="culture"/> is <see
        /// langword="null"/>, <see cref="CultureInfo.InvariantCulture"/> is used.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance to allow method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> UseCulture(CultureInfo culture)
        {
            CultureInfo = culture ?? CultureInfo.InvariantCulture;
            return this;
        }

        /// <summary>
        /// Configures generation options for logging and resource access.
        /// </summary>
        /// <param name="options">The options to use. Cannot be null.</param>
        /// <returns>The current instance for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> UseGenerationOptions(FluidHtmlPdfGenerationOptions options)
        {
            if (options is null) throw new ArgumentNullException(nameof(options));

            GenerationOptions.LogRenderedHtml = options.LogRenderedHtml;
            GenerationOptions.RenderedHtmlLogMaxLength = options.RenderedHtmlLogMaxLength;
            GenerationOptions.ResourceAccessPolicy = options.ResourceAccessPolicy;
            return this;
        }

        /// <summary>
        /// Enables or disables logging of rendered HTML at debug level.
        /// </summary>
        /// <param name="enabled">True to log rendered HTML; otherwise false.</param>
        /// <returns>The current instance for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> UseRenderedHtmlLogging(bool enabled)
        {
            GenerationOptions.LogRenderedHtml = enabled;
            return this;
        }

        /// <summary>
        /// Sets the maximum number of characters to log for rendered HTML.
        /// </summary>
        /// <param name="maxLength">The maximum number of characters to log. Must be greater than zero.</param>
        /// <returns>The current instance for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> UseRenderedHtmlLogMaxLength(int maxLength)
        {
            if (maxLength <= 0) throw new ArgumentOutOfRangeException(nameof(maxLength), "Max length must be greater than zero.");
            GenerationOptions.RenderedHtmlLogMaxLength = maxLength;
            return this;
        }

        /// <summary>
        /// Sets the resource access policy used to restrict external resource loading.
        /// </summary>
        /// <param name="policy">The resource access policy. Null disables restrictions.</param>
        /// <returns>The current instance for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> UseResourceAccessPolicy(HtmlResourceAccessPolicy? policy)
        {
            GenerationOptions.ResourceAccessPolicy = policy;
            return this;
        }

        /// <summary>
        /// Adds a placeholder to the collection of placeholders used for PDF form generation.
        /// </summary>
        /// <param name="placeholder">The placeholder to add to the builder. Cannot be null.</param>
        /// <returns>The current instance of <see cref="FluidHtmlPdfGeneratorBuilder{T}"/>, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddPlaceHolder(BasePlaceHolder<T> placeholder)
        {
            PlaceHolders.Add(placeholder);
            return this;
        }

        /// <summary>
        /// Adds a text placeholder to the PDF form using the specified name and value mapping function.
        /// </summary>
        /// <remarks>If a placeholder with the same name already exists, it will be added again; duplicate
        /// names may result in multiple placeholders in the output. This method is typically used in a fluent
        /// configuration sequence to define multiple placeholders before generating the PDF.</remarks>
        /// <param name="name">The name of the text placeholder to be added. This value is used as the identifier for the placeholder in
        /// the generated PDF form. Cannot be null or empty.</param>
        /// <param name="map">A function that maps an instance of <typeparamref name="T"/> to the string value to be inserted for this
        /// placeholder. Cannot be null.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddText(string name, Func<T, string> map)
        {
            PlaceHolders.Add(new TextPlaceHolder<T>(name, map));
            return this;
        }

        /// <summary>
        /// Adds a numeric placeholder to the PDF form, allowing a decimal value to be mapped and formatted from the
        /// source object.
        /// </summary>
        /// <remarks>If the mapped value is <see langword="null"/>, the placeholder will remain empty in
        /// the generated PDF. The format and culture parameters control how the numeric value appears in the
        /// output.</remarks>
        /// <param name="name">The name of the placeholder to be inserted into the PDF form. This value identifies the field within the
        /// form template.</param>
        /// <param name="map">A function that maps the source object to the decimal value to be displayed in the placeholder. Returns <see
        /// langword="null"/> if no value should be rendered.</param>
        /// <param name="format">The numeric format string used to display the value. Defaults to "N" for standard number formatting.</param>
        /// <param name="culture">The culture information used for formatting the numeric value. If <see langword="null"/>, the builder's
        /// default culture is used.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddCultureNumeric(string name, Func<T, decimal?> map, string format = "N", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new CultureNumericPlaceHolder<T>(name, map, format, culture));
            return this;
        }

        /// <summary>
        /// Adds a numeric placeholder to the PDF form, allowing a decimal value to be mapped from the source object.
        /// </summary>
        /// <remarks>If the mapped value is <see langword="null"/>, the placeholder will remain empty in
        /// the generated PDF. The format and culture parameters control how the numeric value appears in the
        /// output.</remarks>
        /// <param name="name">The name of the placeholder to be inserted into the PDF form. This value identifies the field within the
        /// form template.</param>
        /// <param name="map">A function that maps the source object to the decimal value to be displayed in the placeholder. Returns <see
        /// langword="null"/> if no value should be rendered.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddNumeric(string name, Func<T, decimal?> map)
        {
            PlaceHolders.Add(new NumericPlaceHolder<T>(name, map));
            return this;
        }

        /// <summary>
        /// Adds a placeholder that combines a numeric value and its associated text to the PDF template using the
        /// specified mapping function.
        /// </summary>
        /// <param name="name">The name of the placeholder to be added to the template. This value is used as the identifier within the
        /// PDF.</param>
        /// <param name="map">A function that maps the source object to a <see cref="NumericAndTextValue"/> containing the numeric value
        /// and its corresponding text.</param>
        /// <param name="format">An optional format string that specifies how the numeric value should be formatted. The default is "N".</param>
        /// <param name="culture">An optional <see cref="CultureInfo"/> that determines culture-specific formatting for the numeric value. If
        /// <see langword="null"/>, the current culture is used.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance to allow for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddCultureNumericAndText(string name, Func<T, NumericAndTextValue> map, string format = "N", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new CultureNumericAndTextPlaceHolder<T>(name, map, format, culture));
            return this;
        }

        /// <summary>
        /// Adds a date placeholder to the PDF form using the specified mapping and formatting options.
        /// </summary>
        /// <remarks>If the mapped date value is <see langword="null"/>, the placeholder will be left
        /// empty in the generated PDF. This method can be called multiple times to add multiple date
        /// placeholders.</remarks>
        /// <param name="name">The name of the placeholder to be inserted into the PDF form. Cannot be null or empty.</param>
        /// <param name="map">A function that maps the source object to the date value to be displayed. The function should return a
        /// nullable <see cref="DateTime"/> representing the date, or <see langword="null"/> if no date is available.</param>
        /// <param name="format">The format string used to display the date. Defaults to "G" if not specified. Must be a valid .NET date and
        /// time format string.</param>
        /// <param name="culture">The culture information used for date formatting. If <see langword="null"/>, the default culture of the
        /// builder is used.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddCultureDate(string name, Func<T, DateTime?> map, string format = "G", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new CultureDateTimePlaceHolder<T>(name, map, format, culture));
            return this;
        }

        /// <summary>
        /// Adds a date placeholder to the PDF form using the specified mapping.
        /// </summary>
        /// <remarks>If the mapped date value is <see langword="null"/>, the placeholder will be left
        /// empty in the generated PDF. This method can be called multiple times to add multiple date
        /// placeholders.</remarks>
        /// <param name="name">The name of the placeholder to be inserted into the PDF form. Cannot be null or empty.</param>
        /// <param name="map">A function that maps the source object to the date value to be displayed. The function should return a
        /// nullable <see cref="DateTime"/> representing the date, or <see langword="null"/> if no date is available.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddDate(string name, Func<T, DateTime?> map)
        {
            PlaceHolders.Add(new DateTimePlaceHolder<T>(name, map));
            return this;
        }

        /// <summary>
        /// Adds a flag placeholder with the specified name and mapping function to the PDF generator builder.
        /// </summary>
        /// <param name="name">The name of the flag placeholder to add. Cannot be null or empty.</param>
        /// <param name="map">A function that determines the value of the flag for a given input of type <typeparamref name="T"/>. Cannot
        /// be null.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddFlag(string name, Func<T, bool> map)
        {
            PlaceHolders.Add(new FlagPlaceHolder<T>(name, map));
            return this;
        }

        /// <summary>
        /// Adds a collection placeholder to the PDF generator builder, allowing dynamic insertion of a collection of
        /// items into the generated document.
        /// </summary>
        /// <remarks>Use this method to bind a collection from your model to a named placeholder in the
        /// PDF template. The collection will be rendered in the document where the placeholder is defined. If multiple
        /// collections are added with the same name, only the last one will be used.</remarks>
        /// <param name="name">The name of the collection placeholder to be used in the template. Cannot be null or empty.</param>
        /// <param name="map">A function that maps the model to an enumerable collection of objects to be inserted at the placeholder
        /// location. Cannot be null.</param>
        /// <returns>The current instance of <see cref="FluidHtmlPdfGeneratorBuilder{T}"/>, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddCollection(string name, Func<T, IEnumerable<object>> map)
        {
            PlaceHolders.Add(new CollectionPlaceHolder<T>(name, map));
            return this;
        }

        /// <summary>
        /// Adds a password protection post-processor to the PDF generation pipeline.
        /// </summary>
        /// <param name="masterPassword">The master password for the PDF.</param>
        /// <param name="userPassword">The user password for opening the PDF.</param>
        /// <returns>The current instance of <see cref="FluidHtmlPdfGeneratorBuilder{T}"/>, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddPostPasswordProtect(string masterPassword, string userPassword)
        {
            var passwordProcessor = new PasswordProtectPostProcessor(masterPassword, userPassword);
            PostProcessors.Add(passwordProcessor);
            return this;
        }

        /// <summary>
        /// Adds a post-processing document classifier to the PDF generation pipeline using the specified classification
        /// and optional additional values.
        /// </summary>
        /// <remarks>This method allows you to customize document classification after PDF generation.
        /// Multiple classifiers can be added by calling this method multiple times.</remarks>
        /// <param name="classification">The classification to apply to the generated document. Determines how the document will be categorized
        /// during post-processing.</param>
        /// <param name="additionalValues">An optional dictionary of additional key-value pairs to associate with the classification. Can be null if no
        /// extra values are required.</param>
        /// <returns>The current instance of <see cref="FluidHtmlPdfGeneratorBuilder{T}"/>, enabling method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddPostDocumentClassifier(ClassificationEnum classification, IDictionary<string, string>? additionalValues = null)
        {
            var classifierProcessor = new DocumentClassifierPostProcessor(classification, additionalValues);
            PostProcessors.Add(classifierProcessor);
            return this;
        }

        /// <summary>
        /// Adds a digital signature to the generated PDF using a PFX certificate after the document is created.
        /// </summary>
        /// <remarks>This method applies the digital signature as a post-processing step after PDF
        /// generation. The provided PFX certificate must contain a private key suitable for signing. If multiple
        /// post-processors are added, the order of their execution may affect the final document.</remarks>
        /// <param name="pfxBytes">A byte array containing the PFX (PKCS#12) certificate used to sign the PDF. Cannot be null.</param>
        /// <param name="options">The options that configure the appearance and behavior of the digital signature. Cannot be null.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance for method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddPostPfxDigitalSignature(byte[] pfxBytes, PdfSignatureOptions options)
        {
            var signatureProcessor = new PfxDigitalSignaturePostProcessor(pfxBytes, options);
            PostProcessors.Add(signatureProcessor);
            return this;
        }

        /// <summary>
        /// Adds a post-processing step to be applied after PDF generation.
        /// </summary>
        /// <remarks>Post-processors are executed in the order they are added. This method enables
        /// customization of the PDF output by applying additional processing steps.</remarks>
        /// <param name="postProcessor">An implementation of <see cref="IPostProcessor"/> that defines the post-processing logic to be executed.
        /// Cannot be null.</param>
        /// <returns>The current <see cref="FluidHtmlPdfGeneratorBuilder{T}"/> instance to allow method chaining.</returns>
        public FluidHtmlPdfGeneratorBuilder<T> AddPostProcessor(IPostProcessor postProcessor)
        {
            PostProcessors.Add(postProcessor);
            return this;
        }
    }
}
