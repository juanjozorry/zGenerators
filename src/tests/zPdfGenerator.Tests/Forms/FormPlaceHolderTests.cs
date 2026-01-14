using System;
using System.Globalization;
using zPdfGenerator.Forms.FormPlaceHolders;

namespace zPdfGenerator.Tests.Forms
{
    public class FormPlaceHolderTests
    {
        private sealed class Model
        {
            public decimal? Amount { get; set; }
            public DateTime? When { get; set; }
        }

        [Fact]
        public void NumericPlaceHolder_FormatsValue_WithOverrideCulture()
        {
            var placeholder = new NumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount,
                stringFormat: "N2",
                overrideGlobalCultureInfo: new CultureInfo("es-ES"));

            var result = placeholder.ProcessData(new Model { Amount = 1234.56m }, new CultureInfo("en-US"));

            Assert.Equal("1.234,56", result);
            Assert.Equal("N2", placeholder.StringFormat);
            Assert.Equal("es-ES", placeholder.OverrideGlobalCultureInfo?.Name);
        }

        [Fact]
        public void NumericPlaceHolder_ReturnsEmpty_WhenNull()
        {
            var placeholder = new NumericPlaceHolder<Model>(
                name: "Amount",
                map: m => m.Amount,
                stringFormat: "N2");

            var result = placeholder.ProcessData(new Model { Amount = null }, CultureInfo.InvariantCulture);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void DateTimePlaceHolder_FormatsValue_WithOverrideCulture()
        {
            var placeholder = new DateTimePlaceHolder<Model>(
                name: "When",
                map: m => m.When,
                stringFormat: "dd/MM/yyyy",
                overrideGlobalCultureInfo: new CultureInfo("es-ES"));

            var result = placeholder.ProcessData(new Model { When = new DateTime(2025, 3, 12) }, new CultureInfo("en-US"));

            Assert.Equal("12/03/2025", result);
            Assert.Equal("dd/MM/yyyy", placeholder.StringFormat);
            Assert.Equal("es-ES", placeholder.OverrideGlobalCultureInfo?.Name);
        }

        [Fact]
        public void DateTimePlaceHolder_ReturnsEmpty_WhenNull()
        {
            var placeholder = new DateTimePlaceHolder<Model>(
                name: "When",
                map: m => m.When,
                stringFormat: "dd/MM/yyyy");

            var result = placeholder.ProcessData(new Model { When = null }, CultureInfo.InvariantCulture);

            Assert.Equal(string.Empty, result);
        }
    }
}
