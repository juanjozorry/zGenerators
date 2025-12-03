using System;
using System.Globalization;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace zPdfGenerator.HtmlPlaceHolders
{

    /// <summary>
    /// Class NumericPlaceHolder.
    /// Implements the <see cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" /></summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="zPdfGenerator.HtmlPlaceHolders.BasePlaceHolder{T}" />
    public class ClassedRangeNumericPlaceHolder<T> : NumericPlaceHolder<T>
    {
        private string CssClass { get; set; }
        private decimal? MinValue { get; set; }
        private decimal? MaxValue { get; set; }
        private bool IncludeMinValue { get; set; }
        private bool IncludeMaxValue { get; set; }



        /// <summary>
        /// Initializes a new instance of the <see cref="NumericPlaceHolder{T}" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="map">The map.</param>
        /// <param name="stringFormat">The string format.</param>
        /// <param name="cssClass">The CSS class.</param>
        /// <param name="minValue">The minimum value.</param>
        /// <param name="includeMinValue">if set to <c>true</c> [include minimum value].</param>
        /// <param name="maxValue">The maximum value.</param>
        /// <param name="includeMaxValue">if set to <c>true</c> [include maximum value].</param>
        public ClassedRangeNumericPlaceHolder(string name, Func<T, decimal?> map, string stringFormat = "N", string cssClass = null, decimal? minValue = null, bool includeMinValue = true, decimal? maxValue = null, bool includeMaxValue = true) 
            : base(name, map, stringFormat)
        {
            CssClass = cssClass;
            MinValue = minValue;
            MaxValue = maxValue;
            IncludeMinValue = includeMinValue;
            IncludeMaxValue = includeMaxValue;
        }

        /// <summary>
        /// Processes the node with the place holder and data.
        /// </summary>
        /// <param name="htmlNode">The HtmlNode in where the data will be changed.</param>
        /// <param name="dataItem">The data item.</param>
        /// <param name="cultureInfo">The culture information.</param>
        /// <param name="logger">The logger.</param>
        public override void ProcessNode(HtmlNode htmlNode, T dataItem, CultureInfo cultureInfo, ILogger logger)
        {
            base.ProcessNode(htmlNode, dataItem, cultureInfo, logger);

            var numericValue = Map(dataItem)?.ToString(StringFormat, cultureInfo);
            if (!string.IsNullOrWhiteSpace(CssClass) && (MaxValue != null || MinValue != null) && dataItem != null)
            {
                decimal numberAsDecimal = Convert.ToDecimal(GetNumbers(numericValue));

                bool minValueConditionMet = MinValue == null;
                bool maxValueConditionMet = MaxValue == null;
                
                if(MinValue != null)
                {
                    minValueConditionMet = ((IncludeMinValue && numberAsDecimal >= MinValue) || numberAsDecimal > MinValue);
                }
                
                if (MaxValue != null)
                {
                    maxValueConditionMet = ((IncludeMaxValue && numberAsDecimal <= MaxValue) || numberAsDecimal < MaxValue);
                }
  
                if (minValueConditionMet && maxValueConditionMet)
                {
                    htmlNode.AddClass(CssClass);
                }
            }
        }

        private string GetNumbers(string input)
        {
            return new string(input.Where(c => char.IsDigit(c) || c == '.' || c == ',' || c == '-').ToArray());
        }
    }
}
