using System;
using System.Collections.Generic;
using ClosedXML.Excel;

namespace zExcelGenerator
{
    /// <summary>
    /// ExcelColumnMapper helper class to help us modelling the excel report.
    /// </summary>
    public abstract class ExcelColumnMapper
    {
        /// <summary>
        /// Gets or sets the column order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the format in the resulting report.
        /// </summary>
        /// <value>The format.</value>
        public string Format { get; set; } = "@";

        /// <summary>
        /// Gets or sets a value indicating whether this instance is date.
        /// </summary>
        /// <value><c>true</c> if this instance is date; otherwise, <c>false</c>.</value>
        public XLAlignmentHorizontalValues AlignmentHorizontal { get; set; } = XLAlignmentHorizontalValues.Left;
    }

    /// <summary>
    /// ExcelColumnMapper helper class to help us modelling the excel report.
    /// Implements the <see cref="zExcelGenerator.ExcelColumnMapper" />
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <seealso cref="zExcelGenerator.ExcelColumnMapper" />
    public class ExcelColumnMapper<T> : ExcelColumnMapper
    {
        /// <summary>
        /// Gets or sets the field value. This is a function used to resolve the value for a given column.
        /// </summary>
        /// <value>The field value.</value>
        public Func<T, object> FieldValue { get; set; }
    }

    /// <summary>
    /// Class ExcelMultipleColumnMapper.
    /// Implements the <see cref="zExcelGenerator.ExcelColumnMapper" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zExcelGenerator.ExcelColumnMapper" />
    public class ExcelMultipleColumnMapper<T> : ExcelColumnMapper<T>
    {
        /// <summary>
        /// Gets or sets the header description suffix.
        /// </summary>
        /// <value>The header description suffix.</value>
        public IEnumerable<string> HeaderDescriptionSuffix { get; set; }

        /// <summary>
        /// Gets or sets the field value. This is a function used to resolve the value for a given column.
        /// </summary>
        /// <value>The field value.</value>
        public new Func<T, IEnumerable<object>> FieldValue { get; set; }

        /// <summary>
        /// Gets or sets the total columns.
        /// </summary>
        /// <value>The total columns.</value>
        public int TotalColumns { get; set; }
    }

    /// <summary>
    /// Class ExcelMultipleTwoColumnsMapper.
    /// Implements the <see cref="zExcelGenerator.ExcelColumnMapper" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zExcelGenerator.ExcelColumnMapper" />
    public class ExcelMultipleTwoColumnsMapper<T> : ExcelMultipleColumnMapper<T>
    {
        /// <summary>
        /// Gets or sets if the second column should be shown.
        /// </summary>
        /// <value>The show second column.</value>
        public bool ShowSecondColumn { get; set; }

        /// <summary>
        /// Gets or sets the second column field value.
        /// </summary>
        /// <value>The second column field value.</value>
        public Func<T, IEnumerable<object>> SecondColumnFieldValue { get; set; }

        /// <summary>
        /// Gets or sets the second column description.
        /// </summary>
        /// <value>The second column description.</value>
        public string SecondColumnDescription { get; set; }

        /// <summary>
        /// Gets or sets the second column header description suffix.
        /// </summary>
        /// <value>The second column header description suffix.</value>
        public IEnumerable<string> SecondColumnHeaderDescriptionSuffix { get; set; }

        /// <summary>
        /// Gets or sets the second column format.
        /// </summary>
        /// <value>The second column format.</value>
        public string SecondColumnFormat { get; set; }
    }
}
