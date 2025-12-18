using Microsoft.Extensions.Logging;
using System.Globalization;
using zPdfGenerator.Forms;

namespace zPdfGenerator.Samples.Form
{
    public class FormSample : ISample
    {
        private readonly ILogger<FormSample> _logger;
        private readonly IFormPdfGenerator _generator;

        public FormSample(ILogger<FormSample> logger, IFormPdfGenerator generator)
        {
            _logger = logger;
            _generator = generator;
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting PoC {Time}", DateTimeOffset.Now);

            var sampleData = new FormSampleData
            {
                GivenName = "Juan",
                FamilyName  = "Español Español",
                Address  = "Paseo de la Castallana",
                AddressHouse  = "25",
                AddressAdditional  = "Atico C",
                PostCode  = "28080",
                City = "Madrid",
                Country = "Spain",
                Gender = "Man",
                Height = 150,
                HasLicense = true,
                Deutsch = false,
                English = true,
                French = true,
                Esperanto = false,
                Latin = false,
                FavouriteColor  = "Red"
            };

            var fileContents = _generator.GeneratePdf<FormSampleData>(configure =>
                configure
                    .UseTemplatePath(Path.Combine(AppContext.BaseDirectory, "Form", "template.pdf"))
                    .UseCulture(new CultureInfo("es-ES"))
                    .SetData(sampleData)
                    .SetFlattenFields(true)
                    .AddText("Given Name Text Box", i => i.GivenName)
                    .AddText("Family Name Text Box", i => i.FamilyName)
                    .AddText("Address 1 Text Box", i => i.Address)
                    .AddText("House nr Text Box", i => i.AddressHouse)
                    .AddText("Address 2 Text Box", i => i.AddressAdditional)
                    .AddText("Postcode Text Box", i => i.PostCode)
                    .AddText("City Text Box", i => i.City)
                    .AddText("Country Combo Box", i => i.Country)
                    .AddText("Gender List Box", i => i.Gender)
                    .AddNumeric("Height Formatted Field", i => i.Height)
                    .AddCheckbox("Driving License Check Box", i => i.HasLicense)
                    .AddCheckbox("Language 1 Check Box", i => i.Deutsch)
                    .AddCheckbox("Language 2 Check Box", i => i.English)
                    .AddCheckbox("Language 3 Check Box", i => i.French)
                    .AddCheckbox("Language 4 Check Box", i => i.Esperanto)
                    .AddCheckbox("Language 5 Check Box", i => i.Latin)
                    .AddText("Favourite Colour List Box", i => i.FavouriteColor));

            File.WriteAllBytes(Path.Combine(AppContext.BaseDirectory, "Form\\SampleForm.pdf"), fileContents);

            _logger.LogInformation("Finishing PoC");
        }
    }

    public class FormSampleData
    {
        public string GivenName { get; set; } = ""; 
        public string FamilyName { get; set; } = "";
        public string Address { get; set; } = "";
        public string AddressHouse { get; set; } = "";
        public string AddressAdditional { get; set; } = "";
        public string PostCode { get; set; } = "";
        public string City { get; set; } = "";
        public string Country { get; set; } = "";
        public string Gender { get; set; } = "";
        public decimal? Height { get; set; }
        public bool HasLicense { get; set; } 
        public bool Deutsch { get; set; }
        public bool English { get; set; }
        public bool French { get; set; }
        public bool Esperanto { get; set; }
        public bool Latin { get; set; }
        public string FavouriteColor { get; set; } = "";
    }
}
