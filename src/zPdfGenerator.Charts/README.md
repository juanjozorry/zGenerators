# zPdfGenerator.Charts

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator.Charts?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator.Charts?label=nuget-pre&color=orange)

Extension package for **zPdfGenerator** that adds *chart placeholders* (charts) for PDF generation from HTML using **OxyPlot (SVG)** + **iText**.

The goal of this project is to generate **deterministic, vector-based charts** that render consistently on **Windows and Linux**, following this pipeline:

```
Data → SVG (OxyPlot) → HTML → PDF (iText)
```

---

## Features

* Vector charts (SVG)
* Pie charts
* Color palette support
* Correct culture handling (`es-ES`, `en-US`, etc.)
* PDF-safe layouts (no browser-dependent rendering)
* Clean public API (internal helpers are not exposed to consumers)

---

## Related packages

This project **extends**:

* `zPdfGenerator`

Main dependencies:

* `OxyPlot.Core`
* `iText.Html2Pdf`

---

## Installation

```bash
dotnet add package zPdfGenerator.Charts
```

---

## Basic usage

### 1️ Generator configuration

```csharp
Action<FluidHtmlPdfGeneratorBuilder<CorporateReport>> config = b => b
    .UseTemplatePath("template.html")
    .UseCulture(new CultureInfo("es-ES"))
    .SetData(report)
    .AddPieChart(
        name: "chartSvg",
        map: r => r.TableRows,
        label: i => i.Concept,
        value: i => Convert.ToDouble(i.Value),
        title: "Pie chart example",
        legend: "Distribution",
        paletteHex: new[] { "#2563EB", "#F59E0B", "#16A34A" }
    );
```

### 2️ PDF generation

```csharp
var pdfBytes = generator.GeneratePdf(config);
```

---

## Usage in HTML template

```html
<section class="chart-section">
  <h2>Performance chart</h2>

  <div class="z-chart">
    {{ chartSvg | raw }}
  </div>
</section>
```

---

## Recommended CSS (PDF-friendly)

```css
.chart-section {
  page-break-inside: avoid;
}

.z-chart {
  border: 1px solid #ccc;
  padding: 12px;
  background: #fafafa;
}

.z-chart svg {
  width: 100%;
  height: auto;
  display: block;
}
```

---

## Culture and numeric formatting

OxyPlot formats labels using `CultureInfo.CurrentCulture` at render time.

This project includes an internal helper called **CultureScope** that ensures the correct culture is applied during SVG export:

```csharp
using (CultureScope.Use(culture))
{
    exporter.Export(model, stream);
}
```

This guarantees:

* Correct decimal separators
* Correct percentage formatting
* Independence from the operating system culture

---

## License

MIT License © Juanjo Jiménez Zorrilla

---

## Roadmap

* Bar charts
* Line charts
* Automatic HTML legends

---

> Designed for **serious, reproducible, cross-platform reporting**.
