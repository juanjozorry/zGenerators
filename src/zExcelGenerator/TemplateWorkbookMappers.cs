using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace zExcelGenerator
{
    internal interface ITemplateMapper
    {
        void Apply(ExcelGenerator generator, XLWorkbook workbook, object model, CancellationToken cancellationToken);
    }

    internal sealed class NamedRangeValueMapper<T> : ITemplateMapper
    {
        private readonly string _name;
        private readonly Func<T, object?> _selector;
        private readonly string _format;
        private readonly XLAlignmentHorizontalValues _alignment;
        private readonly string? _worksheetName;

        public NamedRangeValueMapper(string name, Func<T, object?> selector, string format, XLAlignmentHorizontalValues alignment, string? worksheetName = null)
        {
            _name = name;
            _selector = selector;
            _format = format;
            _alignment = alignment;
            _worksheetName = worksheetName;
        }

        public void Apply(ExcelGenerator generator, XLWorkbook workbook, object model, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (model is not T typedModel)
            {
                throw new InvalidOperationException($"Named range '{_name}' expects model type '{typeof(T).Name}'.");
            }

            var namedRanges = TemplateNamedRangeHelpers.FindNamedRanges(workbook, _name).ToList();
            if (!namedRanges.Any())
            {
                throw new InvalidOperationException($"Named range '{_name}' not found.");
            }

            var value = _selector(typedModel);
            var matched = false;
            foreach (var namedRange in namedRanges)
            {
                foreach (var range in namedRange.Ranges)
                {
                    foreach (var cell in range.Cells())
                    {
                        if (!TemplateNamedRangeHelpers.MatchesWorksheet(range, _worksheetName))
                        {
                            continue;
                        }

                        matched = true;
                        cancellationToken.ThrowIfCancellationRequested();
                        generator.SetCellValue(cell, value, _format, _alignment);
                    }
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(_worksheetName))
            {
                throw new InvalidOperationException($"Named range '{_name}' not found in worksheet '{_worksheetName}'.");
            }
        }
    }

    internal sealed class NamedRangeTableMapper<T, TItem> : ITemplateMapper
    {
        private readonly string _name;
        private readonly Func<T, IEnumerable<TItem>> _selector;
        private readonly IEnumerable<ColumnMapper> _mappers;
        private readonly bool _headerRowIsNamedRange;
        private readonly bool _writeHeaders;
        private readonly bool _insertRows;
        private readonly bool _copyTemplateStyle;
        private readonly string? _worksheetName;

        public NamedRangeTableMapper(
            string name,
            Func<T, IEnumerable<TItem>> selector,
            IEnumerable<ColumnMapper> mappers,
            bool headerRowIsNamedRange,
            bool writeHeaders,
            bool insertRows,
            bool copyTemplateStyle,
            string? worksheetName = null)
        {
            _name = name;
            _selector = selector;
            _mappers = mappers;
            _headerRowIsNamedRange = headerRowIsNamedRange;
            _writeHeaders = writeHeaders;
            _insertRows = insertRows;
            _copyTemplateStyle = copyTemplateStyle;
            _worksheetName = worksheetName;
        }

        public void Apply(ExcelGenerator generator, XLWorkbook workbook, object model, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (model is not T typedModel)
            {
                throw new InvalidOperationException($"Named range '{_name}' expects model type '{typeof(T).Name}'.");
            }

            var namedRanges = TemplateNamedRangeHelpers.FindNamedRanges(workbook, _name).ToList();
            if (!namedRanges.Any())
            {
                throw new InvalidOperationException($"Named range '{_name}' not found.");
            }

            var items = _selector(typedModel)?.ToList() ?? new List<TItem>();
            var matched = false;
            foreach (var namedRange in namedRanges)
            {
                foreach (var range in namedRange.Ranges)
                {
                    if (!TemplateNamedRangeHelpers.MatchesWorksheet(range, _worksheetName))
                    {
                        continue;
                    }

                    matched = true;
                    cancellationToken.ThrowIfCancellationRequested();

                    var firstCell = range.FirstCell();
                    if (firstCell is null)
                    {
                        continue;
                    }

                    int headerRow = firstCell.Address.RowNumber;
                    int startColumn = firstCell.Address.ColumnNumber;

                    int dataStartRow;
                    int writeStartRow;
                    if (_headerRowIsNamedRange)
                    {
                        dataStartRow = headerRow + 1;
                        writeStartRow = _writeHeaders ? headerRow : dataStartRow;
                    }
                    else
                    {
                        if (_writeHeaders)
                        {
                            writeStartRow = headerRow;
                            dataStartRow = headerRow + 1;
                        }
                        else
                        {
                            writeStartRow = headerRow;
                            dataStartRow = headerRow;
                        }
                    }

                    if (_insertRows && items.Count > 1)
                    {
                        range.Worksheet.Row(dataStartRow).InsertRowsBelow(items.Count - 1);
                        if (_copyTemplateStyle)
                        {
                            TemplateNamedRangeHelpers.CopyTemplateRowStyle(range.Worksheet, dataStartRow, items.Count, startColumn, TemplateNamedRangeHelpers.GetTotalColumns(_mappers));
                        }
                    }

                    generator.GenerateTableInWorksheet(range.Worksheet, _mappers, items, writeStartRow, startColumn, cancellationToken, _writeHeaders);
                }
            }

            if (!matched && !string.IsNullOrWhiteSpace(_worksheetName))
            {
                throw new InvalidOperationException($"Named range '{_name}' not found in worksheet '{_worksheetName}'.");
            }
        }
    }
}
