using System;
using System.Globalization;
using zPdfGenerator.Forms.FormPlaceHolders;

namespace zPdfGenerator.Tests.Forms
{
    public class CheckboxPlaceHolderTests
    {
        private sealed class Model
        {
            public bool Flag { get; set; }
        }

        [Fact]
        public void ProcessData_ReturnsYes_WhenTrue()
        {
            var placeholder = new CheckboxPlaceHolder<Model>("Flag", m => m.Flag);

            var result = placeholder.ProcessData(new Model { Flag = true }, CultureInfo.InvariantCulture);

            Assert.Equal("Yes", result);
        }

        [Fact]
        public void ProcessData_ReturnsOff_WhenFalse()
        {
            var placeholder = new CheckboxPlaceHolder<Model>("Flag", m => m.Flag);

            var result = placeholder.ProcessData(new Model { Flag = false }, CultureInfo.InvariantCulture);

            Assert.Equal("Off", result);
        }
    }
}
