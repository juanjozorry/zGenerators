using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using zPdfGenerator.Forms;
using zPdfGenerator.Html;
using zPdfGenerator.Samples.Form;
using zPdfGenerator.Samples.Html;

namespace zPdfGenerator.Samples
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Debug);

            builder.Services.AddTransient<ISample, HtmlSample>();
            builder.Services.AddTransient<ISample, FormSample>();
            builder.Services.AddTransient<IFluidHtmlTemplatePdfGenerator, FluidHtmlTemplatePdfGenerator>();
            builder.Services.AddTransient<IHtmlToPdfConverter, HtmlToPdfConverter>();
            builder.Services.AddTransient<IFormPdfGenerator, FormPdfGenerator>();

            using var host = builder.Build();

            var htmlSample = host.Services.GetServices<ISample>().OfType<HtmlSample>().First();
            var formSample = host.Services.GetServices<ISample>().OfType<FormSample>().First();

            await htmlSample.RunAsync();
            await formSample.RunAsync();
        }
    }
}