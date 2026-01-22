# zPdfGenerator.TemplatePreview

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator.TemplatePreview?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator.TemplatePreview?label=nuget-pre&color=orange)

`zPdfGenerator.TemplatePreview` is a **.NET global tool** that provides a fast and generic way to **preview HTML templates** rendered with **Fluid (Liquid syntax)** using sample JSON data.

It is designed to help you **author, debug, and iterate** on HTML templates that will later be converted to PDF (for example using `zPdfGenerator`), without having to write code or regenerate PDFs constantly.

---

## Features

- Preview **any HTML template** using Fluid/Liquid syntax
- Uses a side-by-side `*.sample.json` file as mock data
- Opens the rendered HTML automatically in your browser
- `--watch` mode to re-render instantly on file changes
- Distributed as a **.NET global tool**
- No knowledge of placeholders required — fully generic

---

## Installation

The tool is distributed as a **NuGet .NET Tool**.

### Prerequisites

- .NET 8 SDK or Runtime installed

### Install globally

```bash
dotnet tool install -g zPdfGenerator.TemplatePreview
```

Verify installation:

```bash
zpg-templates --help
```

---

## Usage

### Basic usage

```bash
zpg-templates preview Templates/Invoice.html
```

### Using a full path (with spaces)

```bash
zpg-templates preview "Templates/My Invoice.html"
```

### Watch mode (recommended)

```bash
zpg-templates preview Templates/Invoice.html --watch
```

- Re-renders automatically when the HTML or JSON changes
- Refresh the browser (F5) to see updates

---

## Options

| Option | Description |
|------|------------|
| `--watch` | Re-render when template or JSON changes |
| `--pdf` | Output a PDF instead of opening a browser |
| `--no-open` | Do not open browser automatically |
| `--help` | Show help |

Example:

```bash
zpg-templates preview Templates/Invoice.html --watch 
```

---

## Output and file layout

- The tool expects a sibling `*.sample.json` next to the HTML template.
- Rendered HTML is written to a temporary location and opened in the browser unless `--no-open` is set.
- Use `--pdf` to generate a PDF output for quick visual checks.

---

## Template conventions

### HTML template

Use **Fluid / Liquid syntax**:

```html
<h1>Invoice {{ InvoiceNumber }}</h1>

{% if ShowDiscounts %}
  <div>Discounts section</div>
{% endif %}

<ul>
{% for line in Lines %}
  <li>{{ line.Description }} - {{ line.Price }}</li>
{% endfor %}
</ul>
```

### Sample JSON

```json
{
  "InvoiceNumber": "INV-001",
  "ShowDiscounts": true,
  "Lines": [
    { "Description": "Consulting", "Price": 500 },
    { "Description": "License", "Price": 200 }
  ]
}
```

---

## Troubleshooting

- If nothing renders, confirm the `*.sample.json` file name matches the HTML file name.
- If the browser does not open, run with `--no-open` and open the generated file manually.
- For watch mode, save both the HTML and JSON to trigger re-rendering.

---

## Security Notes

- Treat templates and sample data as trusted local files.
- Avoid opening untrusted HTML with external resource links unless you understand the risks.

---

## Design goals

- No strong typing
- No predefined placeholders
- No coupling to PDF generation
- Works with **any HTML template**
- Optimized for **template authoring and previewing**

---

## Testing

```bash
dotnet test tests/zPdfGenerator.Tests/zPdfGenerator.Tests.csproj
```

---

## Related projects

This tool is part of the **zGenerators** ecosystem:

- **zExcelGenerator**  
  Excel reports with a fluent API  
  https://github.com/juanjozorry/zGenerators

- **zPdfGenerator**  
  PDF generation from Forms and HTML templates  
  https://github.com/juanjozorry/zGenerators

---

## License

MIT License © Juanjo Jiménez Zorrilla
