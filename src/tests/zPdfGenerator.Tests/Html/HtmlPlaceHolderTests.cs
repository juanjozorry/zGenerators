using System;
using System.Globalization;
using zPdfGenerator.Html.FluidHtmlPlaceHolders;

namespace zPdfGenerator.Tests.Html
{
    public class HtmlPlaceHolderTests
    {
        private sealed class Model
        {
            public decimal? Amount { get; set; }
            public DateTime? When { get; set; }
        }

        [Fact]
        public void CultureNumericPlaceHolder_FormatsValue_WithOverrideCulture()
        {
            var placeholder = new CultureNumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount,
                stringFormat: "N2",
                overrideGlobalCultureInfo: new CultureInfo("es-ES"));

            var result = placeholder.ProcessValue(new Model { Amount = 1234.56m }, new CultureInfo("en-US"));

            Assert.Equal("1.234,56", result);
            Assert.Equal("N2", placeholder.StringFormat);
            Assert.Equal("es-ES", placeholder.OverrideGlobalCultureInfo?.Name);
        }

        [Fact]
        public void CultureNumericPlaceHolder_ReturnsNull_WhenNull()
        {
            var placeholder = new CultureNumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount);

            var result = placeholder.ProcessValue(new Model { Amount = null }, CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void NumericPlaceHolder_ReturnsDecimalValue()
        {
            var placeholder = new NumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount);

            var result = placeholder.ProcessValue(new Model { Amount = 12.34m }, CultureInfo.InvariantCulture);

            Assert.IsType<decimal>(result);
            Assert.Equal(12.34m, result);
        }

        [Fact]
        public void NumericPlaceHolder_ReturnsNull_WhenNull()
        {
            var placeholder = new NumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount);

            var result = placeholder.ProcessValue(new Model { Amount = null }, CultureInfo.InvariantCulture);

            Assert.Null(result);
        }

        [Fact]
        public void DateTimePlaceHolder_ReturnsDateTime()
        {
            var placeholder = new DateTimePlaceHolder<Model>(
                name: "When",
                map: m => m.When);

            var date = new DateTime(2025, 3, 12);
            var result = placeholder.ProcessValue(new Model { When = date }, CultureInfo.InvariantCulture);

            Assert.IsType<DateTime>(result);
            Assert.Equal(date, result);
        }

        [Fact]
        public void DateTimePlaceHolder_ReturnsNull_WhenNull()
        {
            var placeholder = new DateTimePlaceHolder<Model>(
                name: "When",
                map: m => m.When);

            var result = placeholder.ProcessValue(new Model { When = null }, CultureInfo.InvariantCulture);

            Assert.Null(result);
        }
    }
}
