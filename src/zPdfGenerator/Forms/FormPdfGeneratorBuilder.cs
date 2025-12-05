using System;
using System.Collections.Generic;
using System.Globalization;
using zPdfGenerator.Forms.FormPlaceHolders;

namespace zPdfGenerator.Forms
{
    /// <summary>
    /// Provides a builder for configuring and creating instances of a PDF generator for forms.
    /// </summary>
    /// <remarks>Use this class to fluently configure options and dependencies required to generate PDF
    /// documents from form data. The builder pattern enables step-by-step customization before constructing the final
    /// PDF generator instance.</remarks>
    public class FormPdfGeneratorBuilder<T>
    {
        /// <summary>
        /// The data item used to resolve all placeholders.
        /// </summary>
        public T? DataItem { get; private set; }

        /// <summary>
        /// Gets the file system path to the template used for rendering or processing operations.
        /// </summary>
        public string TemplatePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the file system path to the license file used by the application.
        /// </summary>
        public string LicensePath { get; private set; } = string.Empty;

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
        /// Gets the list of form element names that should be removed during processing.
        /// </summary>
        internal List<string> FormElementsToRemove { get; private set; } = new();
        
        /// <summary>
        /// Gets a value indicating whether fields are represented in a flattened structure.
        /// </summary>
        public bool FlattenFields { get; private set; }

        /// <summary>
        /// Assign the data item on which placeholders will be processed.
        /// </summary>
        public FormPdfGeneratorBuilder<T> SetData(T data)
        {
            DataItem = data;
            return this;
        }

        /// <summary>
        /// Configures whether form fields in the generated PDF should be flattened, making them non-editable.
        /// </summary>
        /// <remarks>Flattening form fields is useful when you want to prevent further editing of the
        /// fields in the resulting PDF. This setting affects all form fields in the generated document.</remarks>
        /// <param name="flattenFields">A value indicating whether to flatten form fields. Specify <see langword="true"/> to make fields
        /// non-editable; otherwise, specify <see langword="false"/> to keep fields editable.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance with the updated flattening configuration.</returns>
        public FormPdfGeneratorBuilder<T> SetFlattenFields(bool flattenFields)
        {
            FlattenFields = flattenFields;
            return this;
        }

        /// <summary>
        /// Specifies the file system path to the PDF template to be used for form generation.
        /// </summary>
        /// <param name="fullTemplatePath">The full file system path to the PDF template file. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance with the template path configured.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="fullTemplatePath"/> is null, empty, or consists only of white-space characters.</exception>
        public FormPdfGeneratorBuilder<T> UseTemplatePath(string fullTemplatePath)
        {
            if (string.IsNullOrWhiteSpace(fullTemplatePath))
                throw new ArgumentException("Template path cannot be null or empty.", nameof(fullTemplatePath));

            TemplatePath = fullTemplatePath;
            return this;
        }

        /// <summary>
        /// Specifies the path to the license file to be used for PDF generation.
        /// </summary>
        /// <param name="licensePath">The full file system path to the license file. Cannot be null, empty, or consist only of white-space
        /// characters.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance with the license file path configured.</returns>
        /// <exception cref="ArgumentException">Thrown if <paramref name="licensePath"/> is null, empty, or consists only of white-space characters.</exception>
        public FormPdfGeneratorBuilder<T> UseLicenseFile(string licensePath)
        {
            if (string.IsNullOrWhiteSpace(licensePath)) throw new ArgumentException("License path cannot be null or empty.", nameof(licensePath));

            LicensePath = licensePath;

            return this;
        }

        /// <summary>
        /// Sets the culture to be used for formatting and localization operations in the PDF generation process.
        /// </summary>
        /// <param name="culture">The <see cref="CultureInfo"/> to use for formatting and localization. If <paramref name="culture"/> is <see
        /// langword="null"/>, <see cref="CultureInfo.InvariantCulture"/> is used.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance to allow method chaining.</returns>
        public FormPdfGeneratorBuilder<T> UseCulture(CultureInfo culture)
        {
            CultureInfo = culture ?? CultureInfo.InvariantCulture;
            return this;
        }

        /// <summary>
        /// Adds a placeholder to the collection of placeholders used for PDF form generation.
        /// </summary>
        /// <param name="placeholder">The placeholder to add to the builder. Cannot be null.</param>
        /// <returns>The current instance of <see cref="FormPdfGeneratorBuilder{T}"/>, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddPlaceHolder(BasePlaceHolder<T> placeholder)
        {
            PlaceHolders.Add(placeholder);
            return this;
        }

        /// <summary>
        /// Adds a checkbox placeholder to the PDF form using the specified name and value mapping function.
        /// </summary>
        /// <remarks>If a placeholder with the same name already exists, it will be added again; duplicate
        /// names may result in multiple placeholders in the output. This method is typically used in a fluent
        /// configuration sequence to define multiple placeholders before generating the PDF.</remarks>
        /// <param name="name">The name of the text placeholder to be added. This value is used as the identifier for the placeholder in
        /// the generated PDF form. Cannot be null or empty.</param>
        /// <param name="map">A function that maps an instance of <typeparamref name="T"/> to the string value to be inserted for this
        /// placeholder. Cannot be null.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddCheckbox(string name, Func<T, bool> map)
        {
            PlaceHolders.Add(new CheckboxPlaceHolder<T>(name, map));
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
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddText(string name, Func<T, string> map)
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
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddNumeric(string name, Func<T, decimal?> map, string format = "N", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new NumericPlaceHolder<T>(name, map, format, culture ?? CultureInfo));
            return this;
        }

        /// <summary>
        /// Adds a numeric and text placeholder to the PDF form using the specified mapping function and formatting
        /// options.
        /// </summary>
        /// <remarks>Use this method to insert placeholders that display both numeric and textual
        /// information in the generated PDF form. The mapping function should extract the relevant data from the source
        /// object. The format and culture parameters control how the numeric value is rendered.</remarks>
        /// <param name="name">The name of the placeholder to be added to the PDF form. This value is used as the identifier for the
        /// placeholder.</param>
        /// <param name="map">A function that maps the source object to a NumericAndTextValue, providing the numeric and text data for the
        /// placeholder.</param>
        /// <param name="format">The format string used to format the numeric value. Defaults to "N" if not specified.</param>
        /// <param name="culture">The culture information used for formatting the numeric value. If null, the builder's default culture is
        /// used.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddNumericAndText(string name, Func<T, NumericAndTextValue> map, string format = "N", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new NumericAndTextPlaceHolder<T>(name, map, format, culture ?? CultureInfo));
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
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddDate(string name, Func<T, DateTime?> map, string format = "G", CultureInfo? culture = null)
        {
            PlaceHolders.Add(new DateTimePlaceHolder<T>(name, map, format, culture ?? CultureInfo));
            return this;
        }

        /// <summary>
        /// Specifies one or more form element names to be removed from the generated PDF form.
        /// </summary>
        /// <remarks>Calling this method multiple times will accumulate the specified form element names
        /// to be removed. Element names are matched as provided; ensure they correspond to the actual form elements in
        /// the source.</remarks>
        /// <param name="formElementName">An array of form element names to exclude from the PDF output. Each name should correspond to a form element
        /// present in the source.</param>
        /// <returns>The current <see cref="FormPdfGeneratorBuilder{T}"/> instance, enabling method chaining.</returns>
        public FormPdfGeneratorBuilder<T> AddFormElementsToRemove(params string[] formElementName)
        {
            if (formElementName is not null)
            {
                FormElementsToRemove.AddRange(formElementName);
            }
            return this;
        }
    }
}
