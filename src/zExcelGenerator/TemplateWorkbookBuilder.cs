using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace zExcelGenerator
{
    /// <summary>
    /// Builder to populate an Excel template using named ranges.
    /// </summary>
    public sealed class TemplateWorkbookBuilder
    {
        private readonly TemplateWorkbookBuilderState _state;

        internal TemplateWorkbookBuilder(ExcelGenerator generator, CancellationToken cancellationToken)
        {
            _state = new TemplateWorkbookBuilderState(generator, cancellationToken);
        }

        internal TemplateWorkbookBuilder(TemplateWorkbookBuilderState state)
        {
            _state = state;
        }

        /// <summary>
        /// Sets the template path for the workbook.
        /// </summary>
        public TemplateWorkbookBuilder UseTemplatePath(string templatePath)
        {
            if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentException("Template path cannot be null or empty.", nameof(templatePath));

            _state.TemplatePath = templatePath;
            _state.TemplatePathSet = true;
            return this;
        }

        /// <summary>
        /// Sets the data model for the template.
        /// </summary>
        public TemplateWorkbookBuilder<T> SetData<T>(T model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            if (_state.ModelSet && _state.Model is not T)
            {
                throw new InvalidOperationException("Template model type does not match existing model.");
            }

            _state.Model = model;
            _state.ModelSet = true;
            return new TemplateWorkbookBuilder<T>(_state);
        }

        /// <summary>
        /// Adds a new worksheet to the output workbook.
        /// </summary>
        public TemplateWorkbookBuilder AddWorksheet<T>(string reportName, IEnumerable<T> items, Action<WorksheetBuilder<T>> configureColumns, bool includeColumnHeaders = true)
        {
            if (string.IsNullOrWhiteSpace(reportName)) throw new ArgumentException("Report name cannot be null or empty.", nameof(reportName));
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (configureColumns is null) throw new ArgumentNullException(nameof(configureColumns));

            var worksheetBuilder = new WorksheetBuilder<T>();
            configureColumns(worksheetBuilder);
            var mappers = worksheetBuilder.BuildMappers();
            if (!mappers.Any())
            {
                throw new InvalidOperationException($"Worksheet '{reportName}' has no columns configured.");
            }

            _state.Mappers.Add(new WorksheetMapper<T>(reportName, items, includeColumnHeaders, mappers));
            return this;
        }

        internal void ApplyMappers(XLWorkbook workbook)
        {
            if (workbook is null) throw new ArgumentNullException(nameof(workbook));

            foreach (var mapper in _state.Mappers)
            {
                _state.CancellationToken.ThrowIfCancellationRequested();
                mapper.Apply(_state.Generator, workbook, _state.Model ?? throw new InvalidOperationException("Model must be provided via SetData."), _state.CancellationToken);
            }
        }

        internal string GetTemplatePathOrThrow()
        {
            if (!_state.TemplatePathSet || string.IsNullOrWhiteSpace(_state.TemplatePath))
            {
                throw new InvalidOperationException("Template path must be provided via UseTemplatePath.");
            }

            return _state.TemplatePath;
        }

        internal object GetModelOrThrow()
        {
            if (!_state.ModelSet)
            {
                throw new InvalidOperationException("Model must be provided via SetData.");
            }

            return _state.Model!;
        }
    }

    /// <summary>
    /// Typed builder to populate an Excel template using named ranges.
    /// </summary>
    public sealed class TemplateWorkbookBuilder<T>
    {
        private readonly TemplateWorkbookBuilderState _state;

        internal TemplateWorkbookBuilder(TemplateWorkbookBuilderState state)
        {
            _state = state;
        }

        /// <summary>
        /// Sets the template path for the workbook.
        /// </summary>
        public TemplateWorkbookBuilder<T> UseTemplatePath(string templatePath)
        {
            if (string.IsNullOrWhiteSpace(templatePath)) throw new ArgumentException("Template path cannot be null or empty.", nameof(templatePath));

            _state.TemplatePath = templatePath;
            _state.TemplatePathSet = true;
            return this;
        }

        /// <summary>
        /// Sets the data model for the template.
        /// </summary>
        public TemplateWorkbookBuilder<T> SetData(T model)
        {
            if (model is null) throw new ArgumentNullException(nameof(model));

            if (_state.ModelSet && _state.Model is not T)
            {
                throw new InvalidOperationException("Template model type does not match existing model.");
            }

            _state.Model = model;
            _state.ModelSet = true;
            return this;
        }

        /// <summary>
        /// Scopes mappings to a specific worksheet.
        /// </summary>
        public TemplateWorkbookBuilder<T> ForWorksheet(string worksheetName, Action<TemplateWorksheetBuilder<T>> configure)
        {
            if (string.IsNullOrWhiteSpace(worksheetName)) throw new ArgumentException("Worksheet name cannot be null or empty.", nameof(worksheetName));
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            var worksheetBuilder = new TemplateWorksheetBuilder<T>(_state, worksheetName);
            configure(worksheetBuilder);
            return this;
        }

        /// <summary>
        /// Adds a new worksheet to the output workbook.
        /// </summary>
        public TemplateWorkbookBuilder<T> AddWorksheet<TItem>(string reportName, IEnumerable<TItem> items, Action<WorksheetBuilder<TItem>> configureColumns, bool includeColumnHeaders = true)
        {
            if (string.IsNullOrWhiteSpace(reportName)) throw new ArgumentException("Report name cannot be null or empty.", nameof(reportName));
            if (items is null) throw new ArgumentNullException(nameof(items));
            if (configureColumns is null) throw new ArgumentNullException(nameof(configureColumns));

            var worksheetBuilder = new WorksheetBuilder<TItem>();
            configureColumns(worksheetBuilder);
            var mappers = worksheetBuilder.BuildMappers();
            if (!mappers.Any())
            {
                throw new InvalidOperationException($"Worksheet '{reportName}' has no columns configured.");
            }

            _state.Mappers.Add(new WorksheetMapper<TItem>(reportName, items, includeColumnHeaders, mappers));
            return this;
        }

        /// <summary>
        /// Maps a single named range to a value.
        /// </summary>
        public TemplateWorkbookBuilder<T> NamedRange(string name, Func<T, object?> selector, string format = "@", XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Named range cannot be null or empty.", nameof(name));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            _state.Mappers.Add(new NamedRangeValueMapper<T>(name, selector, format, alignment));
            return this;
        }

        /// <summary>
        /// Maps a table to a named range (first cell in the range is used as anchor).
        /// </summary>
        public TemplateWorkbookBuilder<T> NamedRangeTable<TItem>(
            string name,
            Func<T, IEnumerable<TItem>> selector,
            Action<WorksheetBuilder<TItem>> configureColumns,
            bool headerRowIsNamedRange = true,
            bool writeHeaders = false,
            bool insertRows = true,
            bool copyTemplateStyle = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Named range cannot be null or empty.", nameof(name));
            if (selector is null) throw new ArgumentNullException(nameof(selector));
            if (configureColumns is null) throw new ArgumentNullException(nameof(configureColumns));

            var worksheetBuilder = new WorksheetBuilder<TItem>();
            configureColumns(worksheetBuilder);
            var mappers = worksheetBuilder.BuildMappers();
            if (!mappers.Any())
            {
                throw new InvalidOperationException($"Named range '{name}' has no columns configured.");
            }

            _state.Mappers.Add(new NamedRangeTableMapper<T, TItem>(name, selector, mappers, headerRowIsNamedRange, writeHeaders, insertRows, copyTemplateStyle));
            return this;
        }
    }

    /// <summary>
    /// Builder to configure mappings for a specific worksheet.
    /// </summary>
    public sealed class TemplateWorksheetBuilder<T>
    {
        private readonly TemplateWorkbookBuilderState _state;
        private readonly string _worksheetName;

        internal TemplateWorksheetBuilder(TemplateWorkbookBuilderState state, string worksheetName)
        {
            _state = state;
            _worksheetName = worksheetName;
        }

        /// <summary>
        /// Maps a single named range to a value.
        /// </summary>
        public TemplateWorksheetBuilder<T> NamedRange(string name, Func<T, object?> selector, string format = "@", XLAlignmentHorizontalValues alignment = XLAlignmentHorizontalValues.Left)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Named range cannot be null or empty.", nameof(name));
            if (selector is null) throw new ArgumentNullException(nameof(selector));

            _state.Mappers.Add(new NamedRangeValueMapper<T>(name, selector, format, alignment, _worksheetName));
            return this;
        }

        /// <summary>
        /// Maps a table to a named range (first cell in the range is used as anchor).
        /// </summary>
        public TemplateWorksheetBuilder<T> NamedRangeTable<TItem>(
            string name,
            Func<T, IEnumerable<TItem>> selector,
            Action<WorksheetBuilder<TItem>> configureColumns,
            bool headerRowIsNamedRange = true,
            bool writeHeaders = false,
            bool insertRows = true,
            bool copyTemplateStyle = true)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Named range cannot be null or empty.", nameof(name));
            if (selector is null) throw new ArgumentNullException(nameof(selector));
            if (configureColumns is null) throw new ArgumentNullException(nameof(configureColumns));

            var worksheetBuilder = new WorksheetBuilder<TItem>();
            configureColumns(worksheetBuilder);
            var mappers = worksheetBuilder.BuildMappers();
            if (!mappers.Any())
            {
                throw new InvalidOperationException($"Named range '{name}' has no columns configured.");
            }

            _state.Mappers.Add(new NamedRangeTableMapper<T, TItem>(name, selector, mappers, headerRowIsNamedRange, writeHeaders, insertRows, copyTemplateStyle, _worksheetName));
            return this;
        }
    }

    internal sealed class TemplateWorkbookBuilderState
    {
        public TemplateWorkbookBuilderState(ExcelGenerator generator, CancellationToken cancellationToken)
        {
            Generator = generator ?? throw new ArgumentNullException(nameof(generator));
            CancellationToken = cancellationToken;
        }

        public ExcelGenerator Generator { get; }
        public CancellationToken CancellationToken { get; }
        public List<ITemplateMapper> Mappers { get; } = new();
        public string? TemplatePath { get; set; }
        public bool TemplatePathSet { get; set; }
        public object? Model { get; set; }
        public bool ModelSet { get; set; }
    }

    internal static class TemplateNamedRangeHelpers
    {
        public static IEnumerable<IXLDefinedName> FindNamedRanges(XLWorkbook workbook, string name)
        {
            if (workbook is null) throw new ArgumentNullException(nameof(workbook));

            foreach (var definedName in workbook.DefinedNames.Where(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase)))
            {
                yield return definedName;
            }

            foreach (var worksheet in workbook.Worksheets)
            {
                foreach (var definedName in worksheet.DefinedNames.Where(n => string.Equals(n.Name, name, StringComparison.OrdinalIgnoreCase)))
                {
                    yield return definedName;
                }
            }
        }

        public static bool MatchesWorksheet(IXLRange range, string? worksheetName)
        {
            if (string.IsNullOrWhiteSpace(worksheetName))
            {
                return true;
            }

            return string.Equals(range.Worksheet.Name, worksheetName, StringComparison.OrdinalIgnoreCase);
        }

        public static int GetTotalColumns(IEnumerable<ColumnMapper> mappers)
        {
            int total = 0;
            foreach (var mapper in mappers)
            {
                var mapperType = mapper.GetType();
                if (mapperType.IsGenericType && mapperType.GetGenericTypeDefinition() == typeof(MultipleTwoColumnsMapper<>))
                {
                    var totalColumns = (int?)mapperType.GetProperty("TotalColumns")?.GetValue(mapper) ?? 0;
                    var showSecondColumn = (bool?)mapperType.GetProperty("ShowSecondColumn")?.GetValue(mapper) ?? false;
                    total += totalColumns * (showSecondColumn ? 2 : 1);
                    continue;
                }

                if (mapperType.IsGenericType && mapperType.GetGenericTypeDefinition() == typeof(MultipleColumnMapper<>))
                {
                    var totalColumns = (int?)mapperType.GetProperty("TotalColumns")?.GetValue(mapper) ?? 0;
                    total += totalColumns;
                    continue;
                }

                total += 1;
            }

            return total;
        }

        public static void CopyTemplateRowStyle(IXLWorksheet worksheet, int templateRow, int totalRows, int startColumn, int totalColumns)
        {
            if (totalRows <= 1 || totalColumns <= 0)
            {
                return;
            }

            for (int offset = 1; offset < totalRows; offset++)
            {
                var targetRow = templateRow + offset;
                for (int columnOffset = 0; columnOffset < totalColumns; columnOffset++)
                {
                    var sourceCell = worksheet.Cell(templateRow, startColumn + columnOffset);
                    var targetCell = worksheet.Cell(targetRow, startColumn + columnOffset);
                    targetCell.Style = sourceCell.Style;
                }
            }
        }
    }
}
