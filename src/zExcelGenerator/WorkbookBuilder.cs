using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace zExcelGenerator
{
    /// <summary>
    /// This is a helper class to build an Excel workbook with multiple worksheets.
    /// </summary>
    public sealed class WorkbookBuilder
    {
        private readonly ExcelGenerator _generator;
        private readonly XLWorkbook _workbook;
        private readonly CancellationToken _cancellationToken;

        /// <summary>
        /// Initializes a new instance of the WorkbookBuilder class using the specified Excel generator and workbook.
        /// </summary>
        /// <param name="generator">The ExcelGenerator instance used to generate Excel content for the workbook. Cannot be null.</param>
        /// <param name="workbook">The XLWorkbook instance that will be built or modified. Cannot be null.</param>
        /// <param name="cancellationToken">The CancellationToken to observe cancellation requests.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="generator"/> or <paramref name="workbook"/> is null.</exception>
        internal WorkbookBuilder(ExcelGenerator generator, XLWorkbook workbook, CancellationToken cancellationToken)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _workbook = workbook ?? throw new ArgumentNullException(nameof(workbook));
            _cancellationToken = cancellationToken;
        }

        /// <summary>
        /// Añade una hoja al workbook.
        /// </summary>
        /// <typeparam name="T">Tipo de los elementos de la hoja.</typeparam>
        /// <param name="reportName">Nombre de la hoja.</param>
        /// <param name="items">Datos a volcar.</param>
        /// <param name="includeColumnHeaders">Si se deben incluir cabeceras.</param>
        /// <param name="configureColumns">Acción para configurar las columnas.</param>
        /// <returns>El propio <see cref="WorkbookBuilder"/> para seguir encadenando.</returns>
        public WorkbookBuilder AddWorksheet<T>(string reportName, IEnumerable<T> items, bool includeColumnHeaders, Action<WorksheetBuilder<T>> configureColumns)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(reportName)) throw new ArgumentException("Report name cannot be null or empty.", nameof(reportName));
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (configureColumns is null) throw new ArgumentNullException(nameof(configureColumns));

            var worksheetBuilder = new WorksheetBuilder<T>();
            configureColumns(worksheetBuilder);

            _cancellationToken.ThrowIfCancellationRequested();

            var mappers = worksheetBuilder.BuildMappers();
            if (!mappers.Any())
            {
                throw new InvalidOperationException($"Worksheet '{reportName}' has no columns configured.");
            }

            _generator.GenerateWorksheet(_workbook, mappers, reportName, items, _cancellationToken, includeColumnHeaders);

            return this;
        }

        /// <summary>
        /// Overload with includeColumnHeaders defaulting to true.
        /// </summary>
        public WorkbookBuilder AddWorksheet<T>(string reportName, IEnumerable<T> items, Action<WorksheetBuilder<T>> configureColumns) 
            => AddWorksheet(reportName, items, includeColumnHeaders: true, configureColumns);
    }

    /// <summary>
    /// Builder to configure the columns of a worksheet.
    /// </summary>
    /// <typeparam name="T">Type for the elements in a worksheet.</typeparam>
    public sealed class WorksheetBuilder<T>
    {
        private readonly List<ExcelColumnMapper> _mappers = new();

        /// <summary>
        /// Adds a simple collumn to the worksheet.
        /// </summary>
        public WorksheetBuilder<T> Column(string description, Func<T, object> selector, int order, string format = "@", XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
        {
            if (string.IsNullOrWhiteSpace(description)) throw new ArgumentException("Description cannot be null or empty.", nameof(description));
            if (selector is null) throw new ArgumentNullException(nameof(selector)); 

            _mappers.Add(new ExcelColumnMapper<T>
            {
                Order = order,
                Description = description,
                Format = format,
                AlignmentHorizontal = alignment,
                FieldValue = selector
            });

            return this;
        }

        /// <summary>
        /// Allows to add an existing Mapper (ie, ExcelMultipleColumnMapper, ExcelMultipleTwoColumnsMapper, etc.).
        /// </summary>
        public WorksheetBuilder<T> Mapper(ExcelColumnMapper mapper)
        {
            if (mapper is null) throw new ArgumentNullException(nameof(mapper)); 
            _mappers.Add(mapper);
            return this;
        }

        /// <summary>
        /// Column expanding to more physical columns (ExcelMultipleColumnMapper).
        /// </summary>
        public WorksheetBuilder<T> MultipleColumns(string description, Func<T, IEnumerable<object>> selector,
            int totalColumns, int order, IEnumerable<string> headerSuffix = null,
            string format = "@", XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentException("Description cannot be null or empty.", nameof(description));
            if (selector is null)
                throw new ArgumentNullException(nameof(selector));
            if (totalColumns <= 0)
                throw new ArgumentOutOfRangeException(nameof(totalColumns), "TotalColumns must be greater than zero.");

            var mapper = new ExcelMultipleColumnMapper<T>
            {
                Order = order,
                Description = description,
                TotalColumns = totalColumns,
                HeaderDescriptionSuffix = headerSuffix ?? Enumerable.Empty<string>(),
                Format = format,
                AlignmentHorizontal = alignment,
                FieldValue = selector
            };

            _mappers.Add(mapper);
            return this;
        }

        /// <summary>
        /// GRoup of columns (primary + secondary) repeated N times (ExcelMultipleTwoColumnsMapper).
        /// 
        /// Vuisual example by row with totalColumns = 2:
        ///   [Desc1 1] [Desc2 1] [Desc1 2] [Desc2 2]
        /// </summary>
        public WorksheetBuilder<T> TwoColumnsPerField(
            string firstDescription,
            string secondDescription,
            Func<T, IEnumerable<object>> firstSelector,
            Func<T, IEnumerable<object>> secondSelector,
            int totalColumns,
            int order,
            IEnumerable<string> firstHeaderSuffix = null,
            IEnumerable<string> secondHeaderSuffix = null,
            string firstFormat = "@",
            string secondFormat = "@",
            XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left,
            bool showSecondColumn = true)
        {
            if (string.IsNullOrWhiteSpace(firstDescription)) throw new ArgumentException("First description cannot be null or empty.", nameof(firstDescription));
            if (string.IsNullOrWhiteSpace(secondDescription)) throw new ArgumentException("Second description cannot be null or empty.", nameof(secondDescription));
            if (firstSelector is null) throw new ArgumentNullException(nameof(firstSelector));
            if (secondSelector is null) throw new ArgumentNullException(nameof(secondSelector));
            if (totalColumns <= 0) throw new ArgumentOutOfRangeException(nameof(totalColumns), "TotalColumns must be greater than zero.");

            var mapper = new ExcelMultipleTwoColumnsMapper<T>
            {
                Order = order,
                Description = firstDescription,
                SecondColumnDescription = secondDescription,
                TotalColumns = totalColumns,
                HeaderDescriptionSuffix = firstHeaderSuffix ?? Enumerable.Empty<string>(),
                SecondColumnHeaderDescriptionSuffix = secondHeaderSuffix ?? Enumerable.Empty<string>(),
                Format = firstFormat,
                SecondColumnFormat = secondFormat,
                AlignmentHorizontal = alignment,
                ShowSecondColumn = showSecondColumn,
                FieldValue = firstSelector,
                SecondColumnFieldValue = secondSelector
            };

            _mappers.Add(mapper);
            return this;
        }


        internal IEnumerable<ExcelColumnMapper> BuildMappers()
            => _mappers.OrderBy(m => m.Order).ToList();
    }
}
