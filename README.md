
# zGenerators Suite

The **zGenerators Suite** is a set of lightweight, extensible C# libraries designed to simplify the generation of Excel and PDF documents in .NET applications.

This repository contains two core core components:

- **zExcelGenerator** — A fluent API for generating Excel reports using ClosedXML.
- **zPdfGenerator** — A set of tools for generating PDFs from templates:
  - **FormPdfGenerator** — Fill PDF AcroForm templates.
  - **FluidHtmlPdfGenerator** — Render HTML templates (Liquid/Fluid) and transform them into PDF.
- **zPdfGenerator.Charts** — An extension for zPdfGenerator that adds chart placeholders for PDF generation from HTML using OxyPlot (SVG) + iText.
- **zPdfGenerator.TemplatePreview** — A tool to preview Fluid HTML templates in real-time.
  
Each project is documented in detail in its own README.  
This root README provides an overview and links to each component.

---

## Projects Included

### 1. zExcelGenerator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zExcelGenerator?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zExcelGenerator?label=nuget-pre&color=orange)

A fluent and highly customizable engine for generating Excel files with ClosedXML.

Supports:
- Dynamic column mapping  
- Custom formatting (numeric, dates, alignment)  
- Multi-column mappers  
- Automatic column sizing  
- Fluent report builder  
- Async generation with cancellation tokens  
- Template-based generation with named ranges  
- Mixed template + new worksheet generation  

**Documentation:**  
[See zExcelGenerator README](./src/zExcelGenerator/README.md)

---

### 2. zPdfGenerator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator?label=nuget-pre&color=orange)

A modular system for generating PDFs in two different ways:

---

#### **FormPdfGenerator**

Fill out existing **PDF forms (AcroForms)** using strongly typed placeholders.

Supports:
- Text, numeric, date, numeric+text placeholders  
- Culture-aware formatting  
- Removing form fields  
- iText license integration  
- Fluent builder for defining mappings  

**Documentation:**  
[See FormPdfGenerator section in zPdfGenerator README](./src/zPdfGenerator/README.md)

---

#### **FluidHtmlPdfGenerator**

Generate PDFs from **HTML templates** using the Fluid (Liquid-based) template engine.

✔ Supports:
- Strongly typed placeholders  
- Flags for conditional HTML rendering  
- Collections for loops/tables  
- Stylesheets and layout support  
- Full HTML rendering (with custom PDF converter)  

▶ **Documentation:**  
[See FluidHtmlPdfGenerator section in zPdfGenerator README](./src/zPdfGenerator/README.md)

---

### 3. zPdfGenerator.Charts

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator.Charts?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator.Charts?label=nuget-pre&color=orange)

An extension for zPdfGenerator that adds *chart placeholders* (charts) for PDF generation from HTML using **OxyPlot (SVG)** + **iText**.

---

### 2. zPdfGenerator.TemplatePreview

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator.TemplatePreview?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator.TemplatePreview?label=nuget-pre&color=orange)

A tool to help debugging a HTML template in real time.

---

## Shared Design Philosophy

All generators follow these core principles:

### Fluent API
Your document definitions read like a DSL:

```csharp
builder
    .UseTemplatePath("Templates/Invoice.html")
    .SetData(invoice)
    .AddText("CustomerName", x => x.Customer.Name)
    .AddCollection("Lines", x => x.Lines)
    .AddFlag("ShowDiscount", x => x.HasDiscount);
```

### Placeholder-based mapping
The engines focus on **model → template** binding using small reusable placeholder classes.

### Separation of concerns
- Placeholders: formatting & data mapping  
- Builder: document configuration  
- Generator: orchestration  
- Renderer/Converter: output formatting (HTML→PDF, Excel→file)  

---

## Documentation Index

| Project | Description | README |
|--------|-------------|--------|
| **zExcelGenerator** | Excel report generation using ClosedXML | [Open README](./src/zExcelGenerator/README.md) |
| **zPdfGenerator** | PDF form filling using AcroForms and HTML-to-PDF rendering using Liquid templates | [Open README](./src/zPdfGenerator/README.md) |
| **zPdfGenerator.Charts** | Extension to zPdfGenerator to help adding charts to a PDF | [Open README](./src/zPdfGenerator.Charts/README.md) |
| **zPdfGenerator.TemplatePreview** | Tool to help debugging a HTML template in real time | [Open README](./src/zPdfGenerator.TemplatePreview/README.md) |

---

## Contributing

Contributions are welcome!  
Feel free to open issues, submit PRs, or propose enhancements.

---

## License

This project is licensed under the **MIT License**.  
See the LICENSE file for details for each project.

---

## Acknowledgements

- **ClosedXML** for Excel generation  
- **Fluid (Liquid)** for HTML template rendering  
- **iText** for PDF processing  
- **OxyPlot (SVG)** for chart generation

---

Happy generating!
