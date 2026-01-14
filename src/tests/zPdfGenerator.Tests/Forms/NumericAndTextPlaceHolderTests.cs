using System;
using System.Globalization;
using zPdfGenerator.Forms.FormPlaceHolders;

namespace zPdfGenerator.Tests.Forms
{
    public class NumericAndTextPlaceHolderTests
    {
        private sealed class Model
        {
            public decimal? Amount { get; set; }
            public string Currency { get; set; } = string.Empty;
        }

        [Fact]
        public void NumericAndTextValue_SetsProperties()
        {
            var value = new NumericAndTextValue(10.5m, "EUR");

            Assert.Equal(10.5m, value.NumericValue);
            Assert.Equal("EUR", value.TextValue);
        }

        [Fact]
        public void ProcessData_FormatsValue_WithOverrideCulture()
        {
            var placeholder = new NumericAndTextPlaceHolder<Model>(
                name: "Total",
                map: m => new NumericAndTextValue(m.Amount, m.Currency),
                stringFormat: "N2",
                overrideGlobalCultureInfo: new CultureInfo("es-ES"));

            var result = placeholder.ProcessData(new Model { Amount = 1234.56m, Currency = "EUR" }, new CultureInfo("en-US"));

            Assert.Equal("1.234,56 EUR", result);
        }

        [Fact]
        public void ProcessData_ReturnsEmpty_WhenNumericIsNull()
        {
            var placeholder = new NumericAndTextPlaceHolder<Model>(
                name: "Total",
                map: m => new NumericAndTextValue(m.Amount, m.Currency),
                stringFormat: "N2");

            var result = placeholder.ProcessData(new Model { Amount = null, Currency = "EUR" }, CultureInfo.InvariantCulture);

            Assert.Equal(string.Empty, result);
        }
    }
}
