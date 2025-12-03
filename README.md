
# zGenerators Suite

The **zGenerators Suite** is a set of lightweight, extensible C# libraries designed to simplify the generation of Excel and PDF documents in .NET applications.

This repository contains two core core components:

- **zExcelGenerator** — A fluent API for generating Excel reports using ClosedXML.
- **zPdfGenerator** — A set of tools for generating PDFs from templates:
  - **FormPdfGenerator** — Fill PDF AcroForm templates.
  - **HtmlPdfGenerator** — Render HTML templates (Liquid/Fluid) and transform them into PDF.

Each project is documented in detail in its own README.  
This root README provides an overview and links to each component.

---

## 📦 Projects Included

### 1. zExcelGenerator

A fluent and highly customizable engine for generating Excel files with ClosedXML.

✔ Supports:
- Dynamic column mapping  
- Custom formatting (numeric, dates, alignment)  
- Multi-column mappers  
- Automatic column sizing  
- Fluent report builder  
- Async generation with cancellation tokens  

▶ **Documentation:**  
[See zExcelGenerator README](./src/zExcelGenerator/README.md)

---

### 2. zPdfGenerator

A modular system for generating PDFs in two different ways:

---

#### 📄 **FormPdfGenerator**

Fill out existing **PDF forms (AcroForms)** using strongly typed placeholders.

✔ Supports:
- Text, numeric, date, numeric+text placeholders  
- Culture-aware formatting  
- Removing form fields  
- iText license integration  
- Fluent builder for defining mappings  

▶ **Documentation:**  
[See FormPdfGenerator section in zPdfGenerator README](./src/zPdfGenerator/README.md)

---

#### 🌐 **FluidHtmlPdfGenerator**

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

## 🔧 Shared Design Philosophy

All generators follow these core principles:

### ✔ Fluent API
Your document definitions read like a DSL:

```csharp
builder
    .UseTemplatePath("Templates/Invoice.html")
    .SetData(invoice)
    .AddText("CustomerName", x => x.Customer.Name)
    .AddCollection("Lines", x => x.Lines)
    .AddFlag("ShowDiscount", x => x.HasDiscount);
```

### ✔ Placeholder-based mapping
The engines focus on **model → template** binding using small reusable placeholder classes.

### ✔ Separation of concerns
- Placeholders: formatting & data mapping  
- Builder: document configuration  
- Generator: orchestration  
- Renderer/Converter: output formatting (HTML→PDF, Excel→file)  

---

## 📚 Documentation Index

| Project | Description | README |
|--------|-------------|--------|
| **zExcelGenerator** | Excel report generation using ClosedXML | [Open README](./src/zExcelGenerator/README.md) |
| **FormPdfGenerator** | PDF form filling using AcroForms | [Open README](./src/zPdfGenerator/README.md) |
| **FluidHtmlPdfGenerator** | HTML-to-PDF rendering using Liquid templates | [Open README](./src/zPdfGenerator/README.md) |

---

## 🚀 Roadmap

- Add CLI tools for batch generation  

---

## 🤝 Contributing

Contributions are welcome!  
Feel free to open issues, submit PRs, or propose enhancements.

---

## 📄 License

This project is licensed under the **MIT License**.  
See the LICENSE file for details.

---

## 🙌 Acknowledgements

- **ClosedXML** for Excel generation  
- **Fluid (Liquid)** for HTML template rendering  
- **iText** for PDF processing  

---

Happy generating! 🚀
