using ClosedXML.Excel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace zExcelGenerator
{
    /// <summary>
    /// Class ExcelGeneratorService.
    /// </summary>
    public class ExcelGenerator
    {
        private readonly ILogger<ExcelGenerator> _logger;
        //private const string _fileExtension = ".xlsx";
        //private const string _mediaTypeHeaderValue = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// Initializes a new instance of the <see cref="ExcelGenerator" /> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        public ExcelGenerator(ILogger<ExcelGenerator> logger)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Generates an Excel file.
        /// </summary>
        /// <param name="configure">Action to configure the Excel generator.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Array of bytes with the content of the file.</returns>
        public byte[] GenerateExcel(Action<WorkbookBuilder> configure, CancellationToken cancellationToken = default)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure));

            cancellationToken.ThrowIfCancellationRequested();

            using (var workbook = CreateWorkbook())
            {
                _logger.LogInformation("Starting Excel generation (fluent API).");

                var builder = new WorkbookBuilder(this, workbook, cancellationToken);
                configure(builder);

                cancellationToken.ThrowIfCancellationRequested();

                var result = ToByteArray(workbook);

                _logger.LogInformation("Finished Excel generation (fluent API). File size: {Size} bytes.", result.Length);

                return result;
            }
        }

        /// <summary>
        /// Generates an Excel file.
        /// </summary>
        /// <param name="configure">Action to configure the Excel generator.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Array of bytes with the content of the file.</returns>
        public Stream GenerateExcelAsStream(Action<WorkbookBuilder> configure, CancellationToken cancellationToken = default)
        {
            if (configure is null) throw new ArgumentNullException(nameof(configure)); 
            
            cancellationToken.ThrowIfCancellationRequested();

            using (var workbook = CreateWorkbook())
            {
                _logger.LogInformation("Starting Excel generation (fluent API).");

                var builder = new WorkbookBuilder(this, workbook, cancellationToken);
                configure(builder);

                cancellationToken.ThrowIfCancellationRequested();

                var result = ToStream(workbook);

                _logger.LogInformation("Finished Excel generation (fluent API). File size: {Size} bytes.", result.Length);

                return result;
            }
        }

        /// <summary>
        /// Async version for <see cref="GenerateExcel(Action{WorkbookBuilder}, CancellationToken)"/>.
        /// </summary>
        public Task<byte[]> GenerateExcelAsync(Action<WorkbookBuilder> configure, CancellationToken cancellationToken = default)
            => Task.Run(() => GenerateExcel(configure, cancellationToken));

        /// <summary>
        /// Async version for <see cref="GenerateExcel(Action{WorkbookBuilder}, CancellationToken)"/>.
        /// </summary>
        public Task<Stream> GenerateExcelAsStreamAsync(Action<WorkbookBuilder> configure, CancellationToken cancellationToken = default)
            => Task.Run(() => GenerateExcelAsStream(configure, cancellationToken));

        /// <summary>
        /// Generates the worksheet.
        /// </summary>
        /// <typeparam name="T">The type in the worksheet.</typeparam>
        /// <param name="workbook">The excel package.</param>
        /// <param name="columnMappers">The column mappers name.</param>
        /// <param name="reportName">The report name.</param>
        /// <param name="items">The items.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="includeColumnHeaders">If column names must be added.</param>
        public void GenerateWorksheet<T>(XLWorkbook workbook, IEnumerable<ExcelColumnMapper> columnMappers, string reportName, IEnumerable<T> items, CancellationToken cancellationToken, bool includeColumnHeaders = true)
        {
            var sw = Stopwatch.StartNew();

            _logger.LogInformation($"Starting worksheet {reportName} generation.");
            var mapperColumns = columnMappers.OrderBy(mc => mc.Order);


            var worksheet = workbook.Worksheets.Add(reportName);

            // Creates header row depending on the mapper columns
            var column = 1;
            if (includeColumnHeaders)
            {
                foreach (var item in mapperColumns)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (item is ExcelMultipleTwoColumnsMapper<T> twomi)
                    {
                        for (int i = 0; i < twomi.TotalColumns; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var desc = twomi.HeaderDescriptionSuffix.ElementAtOrDefault(i) ?? (i + 1).ToString();
                            worksheet.Cell(1, column).SetValue($"{item.Description} {desc}");
                            worksheet.Cell(1, column).Style.Font.Bold = true;
                            column++;
                            if (twomi.ShowSecondColumn)
                            {
                                worksheet.Cell(1, column).SetValue($"{item.Description} {desc}");
                                var secondDesc = twomi.SecondColumnHeaderDescriptionSuffix.ElementAtOrDefault(i) ?? (i + 1).ToString();
                                worksheet.Cell(1, column).SetValue($"{twomi.SecondColumnDescription} {secondDesc}");
                                worksheet.Cell(1, column).Style.Font.Bold = true;
                                column++;
                            }
                        }
                    }
                    else if (item is ExcelMultipleColumnMapper<T> mi)
                    {
                        for (int i = 0; i < mi.TotalColumns; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            var desc = mi.HeaderDescriptionSuffix.ElementAtOrDefault(i) ?? (i + 1).ToString();
                            worksheet.Cell(1, column).SetValue($"{item.Description} {desc}");
                            worksheet.Cell(1, column).Style.Font.Bold = true;
                            column++;
                        }
                    }
                    else if (item is ExcelColumnMapper<T>)
                    {
                        worksheet.Cell(1, column).SetValue(item.Description);
                        worksheet.Cell(1, column).Style.Font.Bold = true;
                        column++;
                    }
                }
            }

            int row = 0;
            int topGap = includeColumnHeaders ? 2 : 1;

            foreach (var currentValue in items)
            {
                column = 1;
                foreach (var item in mapperColumns)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (item is ExcelMultipleTwoColumnsMapper<T> twomi)
                    {
                        var data = twomi.FieldValue?.Invoke(currentValue);
                        var secondColumnData = twomi.SecondColumnFieldValue?.Invoke(currentValue);
                        for (int i = 0; i < twomi.TotalColumns; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            SetCellValue(worksheet.Cell(row + topGap, column), data.ElementAtOrDefault(i), twomi.Format, twomi.AlignmentHorizontal);
                            column++;
                            if (twomi.ShowSecondColumn)
                            {
                                SetCellValue(worksheet.Cell(row + topGap, column), secondColumnData.ElementAtOrDefault(i), twomi.SecondColumnFormat, twomi.AlignmentHorizontal);
                                column++;
                            }
                        }
                    }
                    else if (item is ExcelMultipleColumnMapper<T> mi)
                    {
                        var data = mi.FieldValue?.Invoke(currentValue);
                        for (int i = 0; i < mi.TotalColumns; i++)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            SetCellValue(worksheet.Cell(row + topGap, column), data.ElementAtOrDefault(i), mi.Format, mi.AlignmentHorizontal);
                            column++;
                        }
                    }
                    else if (item is ExcelColumnMapper<T> i)
                    {
                        var data = i.FieldValue?.Invoke(currentValue);
                        if (data is not null) SetCellValue(worksheet.Cell(row + topGap, column), data, i.Format, i.AlignmentHorizontal);
                        column++;
                    }
                }

                row++;
            }

            worksheet.Columns().AdjustToContents();

            sw.Stop();
            _logger.LogInformation($"Finished worksheet {reportName} generation. Rows added {row}. Elapsed {sw.ElapsedMilliseconds} ms.");
        }

        /// <summary>
        /// Creates the workbook.
        /// </summary>
        /// <returns>XLWorkbook.</returns>
        protected static XLWorkbook CreateWorkbook()
        {
            return new XLWorkbook();
        }

        /// <summary>
        /// Exports the excel.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <returns>Returns the Excel contents as an array of bytes.</returns>
        protected static byte[] ToByteArray(XLWorkbook workbook)
        {
            using (var memoryStream = new MemoryStream())
            {
                workbook.SaveAs(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Exports the excel to a file stream result.
        /// </summary>
        /// <param name="workbook">The workbook.</param>
        /// <returns>Returns a FileStreamResult with the Excel contents</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static Stream ToStream(XLWorkbook workbook)
        {
            if (workbook == null) throw new ArgumentNullException(nameof(workbook));

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Gets the proper numeric value.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="originalColumnValue">The original column value.</param>
        /// <returns>System.Object.</returns>
        private void SetProperValue(IXLCell cell, object originalColumnValue)
        {
            if (originalColumnValue is string && double.TryParse(originalColumnValue?.ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double decimalValue))
            {
                cell.Value = decimalValue;
            }
            if (originalColumnValue is string && DateTime.TryParse(originalColumnValue?.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime dateTimeValue))
            {
                cell.Value = dateTimeValue;
            }
            else if (originalColumnValue is decimal decValue)
            {
                cell.Value = (double)decValue;

            }
            else if (originalColumnValue is int intValue)
            {
                cell.Value = intValue;
            }
            else if (originalColumnValue is double doubleValue)
            {
                cell.Value = doubleValue;
            }
            else if (originalColumnValue is DateTime dtValue)
            {
                cell.Value = dtValue;
            }
            else if (originalColumnValue is TimeSpan timeSpanValue)
            {
                cell.Value = timeSpanValue;
            }
            else
            {
                cell.Value = originalColumnValue?.ToString();
            }
        }

        /// <summary>
        /// Sets the cell value.
        /// </summary>
        /// <param name="cell">The cell.</param>
        /// <param name="value">The value.</param>
        /// <param name="format">The format.</param>
        /// <param name="alignmentHorizontal">The alignmentHorizontal</param>
        private void SetCellValue(IXLCell cell, object value, string? format, XLAlignmentHorizontalValues alignmentHorizontal)
        {
            if (cell is not null)
            {
                cell.Style.NumberFormat.SetFormat(format);
                cell.Style.Alignment.Horizontal = alignmentHorizontal;
                SetProperValue(cell, value);
            }
        }
    }
}