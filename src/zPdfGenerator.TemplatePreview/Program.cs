using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using zPdfGenerator.Html;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        services.AddTransient<IFluidHtmlTemplatePdfGenerator, FluidHtmlTemplatePdfGenerator>();
        services.AddTransient<IHtmlToPdfConverter, HtmlToPdfConverter>();
        services.AddTransient<IPreviewService, PreviewService>();

        using var serviceProvider = services.BuildServiceProvider();

        var root = BuildCommandLine(serviceProvider);
        return await root.Parse(args).InvokeAsync();
    }

    private static RootCommand BuildCommandLine(IServiceProvider services)
    {
        var templateArg = new Argument<string>("template");
        templateArg.Description = "Template name (e.g. Invoice) or path to a .html file (wrap in quotes if it contains spaces).";

        var watchOpt = new Option<bool>("--watch") { Description = "Watch template + sample JSON and re-render on changes." };
        var pdfOpt = new Option<bool>("--pdf") { Description = "Generate PDF preview (requires a configured IPdfGenerator)." };
        var noOpenOpt = new Option<bool>("--no-open") { Description = "Do not open the generated preview." };

        var previewCmd = new Command("preview", "Render a Fluid/Liquid HTML template using its .sample.json next to it.")
        {
            templateArg, watchOpt, pdfOpt, noOpenOpt
        };

        previewCmd.SetAction(async parseResult =>
        {
            var template = parseResult.GetValue(templateArg);
            var watch = parseResult.GetValue(watchOpt);
            var pdf = parseResult.GetValue(pdfOpt);
            var noOpen = parseResult.GetValue(noOpenOpt);

            var svc = services.GetRequiredService<IPreviewService>();
            return await svc.RunAsync(template, watch, pdf, noOpen, CancellationToken.None);
        });

        var root = new RootCommand("zGenerators template preview tool")
        {
            previewCmd
        };
        return root;
    }
}

internal interface IPreviewService
{
    Task<int> RunAsync(string? templateInput, bool watch, bool pdf, bool noOpen, CancellationToken ct);
}

internal sealed class PreviewService : IPreviewService
{
    private readonly ILogger<PreviewService> _logger;
    private readonly IFluidHtmlTemplatePdfGenerator _pdfGenerator;

    //    private const string LiveReloadScript = @"
    //<script>
    //(function () {
    //  const CHECK_INTERVAL = 1000; // ms
    //  let lastModified = null;

    //  async function check() {
    //    try {
    //      const res = await fetch(window.location.href, { method: 'HEAD', cache: 'no-store' });
    //      const lm = res.headers.get('Last-Modified');

    //      if (lastModified && lm && lastModified !== lm) {
    //        location.reload();
    //      }
    //      lastModified = lm;
    //    } catch (e) {
    //      // ignore
    //    }
    //  }

    //  setInterval(check, CHECK_INTERVAL);
    //})();
    //</script>";
    private const string LiveReloadScript = @"
<script>
(function () {
  const INTERVAL = 3000; // ms
  setInterval(function () {
    // Fuerza recarga (evita cache)
    window.location.reload();
  }, INTERVAL);
})();
</script>";

    public PreviewService(ILogger<PreviewService> logger, IFluidHtmlTemplatePdfGenerator pdfGenerator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfGenerator = pdfGenerator ?? throw new ArgumentNullException(nameof(pdfGenerator)); ;
    }

    public async Task<int> RunAsync(string? templateInput, bool watch, bool pdf, bool noOpen, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrEmpty(templateInput))
            {
                Console.Error.WriteLine($"ERROR: Template input not provided.");
                return 1;
            }

            var templatePath = Path.GetFullPath(templateInput);

            Console.WriteLine($"Template: {templatePath}");

            if (!File.Exists(templatePath))
            {
                Console.Error.WriteLine($"ERROR: Template not found: {templatePath}");
                return 1;
            }

            var sampleJsonPath = GetSampleJsonPath(templatePath);
            if (!File.Exists(sampleJsonPath))
            {
                Console.Error.WriteLine($"ERROR: Sample JSON not found: {sampleJsonPath}");
                return 1;
            }

            Console.WriteLine($"Sample JSON: {sampleJsonPath}");

            var templateText = File.ReadAllText(templatePath, Encoding.UTF8);
            var parser = new FluidParser();
            if (!parser.TryParse(templateText, out var template, out var error))
            {
                Console.WriteLine($"ERROR parsing the template: {error}");
                return 1;
            }

            var templateDir = Path.GetDirectoryName(templatePath)!;
            var htmlPreviewFileName = $"__preview__{Guid.NewGuid():N}.html";
            var htmlOutputPath = Path.Combine(templateDir, htmlPreviewFileName);
            var pdfPreviewFileName = $"__preview__{Guid.NewGuid():N}.pdf";
            var pdfOutputPath = Path.Combine(templateDir, pdfPreviewFileName);

            Console.WriteLine($"Template: {templatePath}");
            Console.WriteLine($"Sample:   {sampleJsonPath}");
            Console.WriteLine();

            RenderToHtml(templatePath, sampleJsonPath, htmlOutputPath, watch);
            if (pdf)
            {
                RenderToPdf(templatePath, sampleJsonPath, pdfOutputPath);
            }

            if (!noOpen)
            {
                OpenInBrowser(htmlOutputPath);
                if (pdf)
                {
                    OpenInBrowser(pdfOutputPath);
                }
            }

            if (!watch)
                return 0;

            Console.WriteLine("--watch enabled. Edit template or sample JSON and refresh browser (F5).");
            Console.WriteLine("HTML preview auto-reloads in browser.");
            if (pdf)
            {
                Console.WriteLine("PDF preview requires manual refresh (F5).");
            }
            Console.WriteLine("Press Ctrl+C to exit.");

            await WatchLoopAsync(templatePath, sampleJsonPath, htmlOutputPath, pdf, pdfOutputPath);

            foreach (var file in Directory.GetFiles(templateDir, "__preview__*.html"))
            {
                try { File.Delete(file); } catch { /* ignore */ }
            }
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Unexpected error:");
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private async Task WatchLoopAsync(string templatePath, string jsonPath, string htmlOutputPath, bool pdf, string pdfOutputPath)
    {
        var dir = Path.GetDirectoryName(templatePath)!;

        using var debounce = new DebounceDispatcher(TimeSpan.FromMilliseconds(250), () =>
        {
            try
            {
                RenderToHtml(templatePath, jsonPath, htmlOutputPath, true);
                if (pdf)
                {
                    RenderToPdf(templatePath, jsonPath, pdfOutputPath);
                }
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Render error: {ex.Message}");
            }
        });

        using var watcher = new FileSystemWatcher(dir)
        {
            IncludeSubdirectories = false,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size
        };

        watcher.Changed += (_, e) => { if (IsRelevant(e.FullPath, templatePath, jsonPath)) debounce.Trigger(); };
        watcher.Created += (_, e) => { if (IsRelevant(e.FullPath, templatePath, jsonPath)) debounce.Trigger(); };
        watcher.Renamed += (_, e) => { if (IsRelevant(e.FullPath, templatePath, jsonPath)) debounce.Trigger(); };

        var tcs = new TaskCompletionSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            tcs.TrySetResult();
        };

        await tcs.Task;
    }

    private bool IsRelevant(string changedPath, string templatePath, string jsonPath)
    {
        var full = Path.GetFullPath(changedPath);
        return string.Equals(full, Path.GetFullPath(templatePath), StringComparison.OrdinalIgnoreCase)
            || string.Equals(full, Path.GetFullPath(jsonPath), StringComparison.OrdinalIgnoreCase);
    }

    private void RenderToHtml(string templatePath, string jsonPath, string outputPath, bool watch)
    {
        var templateText = File.ReadAllText(templatePath, Encoding.UTF8);

        var model = LoadModelFromJson(jsonPath);

        var html = _pdfGenerator.RenderHtml(templatePath, model, CultureInfo.CurrentCulture, CancellationToken.None);
        if (watch)
        {
            html += LiveReloadScript;
        }
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllText(outputPath, html, Encoding.UTF8);
    }

    private void RenderToPdf(string templatePath, string jsonPath, string outputPath)
    {
        var templateText = File.ReadAllText(templatePath, Encoding.UTF8);

        var model = LoadModelFromJson(jsonPath);

        var pdf = _pdfGenerator.GeneratePdf(templatePath, null, model, CultureInfo.InvariantCulture, CancellationToken.None);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        File.WriteAllBytes(outputPath, pdf);
    }


    private string GetSampleJsonPath(string templatePath)
    {
        var dir = Path.GetDirectoryName(templatePath)!;
        var name = Path.GetFileNameWithoutExtension(templatePath);
        return Path.Combine(dir, name + ".sample.json");
    }

    private void OpenInBrowser(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        });
    }

    private static IDictionary<string, object?> LoadModelFromJson(string jsonPath)
    {
        var json = File.ReadAllText(jsonPath, Encoding.UTF8);
        var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("Invalid JSON.");

        var obj = ToObjectGraph(node);
        if (obj is not IDictionary<string, object?> dict)
            throw new InvalidOperationException("Sample JSON root must be an object.");

        return dict;
    }

    private static object? ToObjectGraph(JsonNode node) =>
        node switch
        {
            JsonObject obj => obj.ToDictionary(kvp => kvp.Key, kvp => kvp.Value is null ? null : ToObjectGraph(kvp.Value)),
            JsonArray arr => arr.Select(n => n is null ? null : ToObjectGraph(n)).ToList(),
            JsonValue val => GetPrimitive(val),
            _ => null
        };

    private static object? GetPrimitive(JsonValue val)
    {
        if (val.TryGetValue<string>(out var s)) return s;
        if (val.TryGetValue<bool>(out var b)) return b;
        if (val.TryGetValue<long>(out var l)) return l;
        if (val.TryGetValue<decimal>(out var d)) return d;
        if (val.TryGetValue<double>(out var db)) return db;
        return val.ToJsonString();
    }

    private sealed class DebounceDispatcher : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly Action _action;
        private Timer? _timer;

        public DebounceDispatcher(TimeSpan delay, Action action)
        {
            _delay = delay;
            _action = action;
        }

        public void Trigger()
        {
            _timer?.Dispose();
            _timer = new Timer(_ => _action(), null, _delay, Timeout.InfiniteTimeSpan);
        }

        public void Dispose() => _timer?.Dispose();
    }
}
