# zPdfGenerator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator?label=nuget-pre&color=orange)

A lightweight, fluent, and extensible PDF generation toolkit for .NET.  
It provides two high-level generators:

---

## 📌 Available Generators

### **1️⃣ FormPdfGenerator (AcroForm PDF filler)**
Fills existing **PDF form templates (AcroForms)** with strongly-typed data models using a fluent builder and placeholder system.

### **2️⃣ FluidHtmlPdfGenerator (HTML → PDF renderer)**
Will generate PDF documents from **Fluid HTML templates** and a strongly typed model.

---

# ✨ Features

### ✔️ Shared features (both generators)
- Fluent API with strongly-typed placeholders  
- Automatic culture-aware formatting (dates, numbers, currencies)  
- Based on .NET 6+  
- Extensible design — add custom placeholders easily  
- Built-in logging via `ILogger<T>`  
- Supports cancellation tokens  
- Fully testable and mockable  

---

# 📦 Installation

```bash
dotnet add package zPdfGenerator
```

### Required for **FormPdfGenerator**
```bash
dotnet add package itext
dotnet add package itext.forms
dotnet add package itext.bouncy-castle-adapter
```

### Required for **HtmlPdfGenerator**
```bash
dotnet add package itext.html2pdf
dotnet add package FluidCore
```

---

# 🚀 FormPdfGenerator

`FormPdfGenerator` fills PDF **AcroForms** using a fluent builder and placeholder mapping.

## Example Model

```csharp
public class CustomerInfo
{
    public string Name { get; set; }
    public DateTime? BirthDate { get; set; }
    public decimal? Balance { get; set; }
    public decimal? Total { get; set; }
    public string Currency { get; set; }
}
```

---

## Simple Example

```csharp
var pdf = new FormPdfGenerator(logger);

byte[] bytes = pdf.GeneratePdf<CustomerInfo>(builder =>
{
    builder
        .UseTemplatePath("templates/Contract.pdf")
        .UseCulture(new CultureInfo("es-ES"))
        .SetData(customer)

        .AddText("Name", c => c.Name)
        .AddDate("BirthDate", c => c.BirthDate, "dd/MM/yyyy")
        .AddNumeric("Balance", c => c.Balance, "N2")
        .AddNumericAndText(
            "TotalWithCurrency",
            c => new NumericAndTextValue(c.Total, c.Currency))

        .AddFormElementsToRemove("OptionalSignature");
});

File.WriteAllBytes("ContractCompleted.pdf", bytes);
```

---

# 📚 Examples of Use

## 📌 Example 1: Basic text fields

```csharp
builder
    .SetData(order)
    .AddText("OrderNumber", o => o.OrderNumber)
    .AddText("CustomerName", o => o.CustomerName)
    .AddText("Notes", o => o.Comments);
```

## 📌 Example 2: Date formatting with culture

```csharp
builder
    .UseCulture(new CultureInfo("es-ES"))
    .AddDate("IssueDate", o => o.CreatedAt, "dd MMMM yyyy")
    .AddDate("ExpiryDate", o => o.ExpiresAt, "dd/MM/yyyy");
```

## 📌 Example 3: Numeric formatting

```csharp
builder
    .UseCulture(new CultureInfo("de-DE"))
    .AddNumeric("Amount", o => o.Amount, "N2")
    .AddNumeric("TaxRate", o => o.TaxRate, "P1");
```

## 📌 Example 4: Composite numeric + text placeholder

```csharp
builder.AddNumericAndText(
    "AmountCurrency",
    o => new NumericAndTextValue(o.Total, o.CurrencyCode),
    "N2"
);
```

## 📌 Example 5: Removing form fields dynamically

```csharp
builder.AddFormElementsToRemove("DebugField", "UnusedField");
```

## 📌 Example 6: Full configuration

```csharp
var pdfBytes = generator.GeneratePdf<Invoice>(builder =>
{
    builder
        .UseTemplatePath("templates/Invoice.pdf")
        .UseLicenseFile("licenses/itextkey.json")
        .UseCulture(new CultureInfo("en-US"))
        .SetData(invoice)

        .AddText("InvoiceNumber", i => i.Number)
        .AddText("CustomerName", i => i.Customer.Name)
        .AddText("Address", i => i.Customer.Address)
        .AddDate("InvoiceDate", i => i.Date, "MMMM dd, yyyy")
        .AddNumeric("Total", i => i.Total, "C2")
        .AddNumericAndText(
            "TotalInWords",
            i => new NumericAndTextValue(i.Total, i.CurrencyCode))

        .AddFormElementsToRemove("OptionalCommentField", "InternalNotes");
});
```

---

# 🧠 Placeholder System Overview

| Placeholder Type | Maps From | Output |
|------------------|-----------|--------|
| `TextPlaceHolder<T>` | `Func<T, string>` | Raw text |
| `NumericPlaceHolder<T>` | `Func<T, decimal?>` | Formatted number |
| `DateTimePlaceHolder<T>` | `Func<T, DateTime?>` | Formatted date |
| `NumericAndTextPlaceHolder<T>` | `Func<T, NumericAndTextValue>` | Composite (e.g., "1,234.00 EUR") |

---

# 🧪 Unit Testing

Creating a template programmatically:

```csharp
var form = PdfFormCreator.GetAcroForm(pdf, true);

new TextFormFieldBuilder(pdf, "Name")
    .SetWidgetRectangle(new Rectangle(50, 750, 200, 20))
    .CreateText();
```

Reading values:

```csharp
var form = PdfAcroForm.GetAcroForm(pdf, false);
var fields = form.GetAllFormFields();

string name = fields["Name"].GetValueAsString();
```

---

# 🚀 FluidHtmlPdfGenerator 

The future HTML engine will render PDFs directly from a Fluid HTML templates.

### Planned features

- Fluid support  
- Strongly typed model binding  
- Shared placeholder API  
- Embedded assets (CSS, fonts, images)  
- Page layout configuration (margins, headers, footers)  

### Planned example

```csharp
var pdf = new FluidHtmlPdfGenerator(logger, new HtmlToPdfConverter());

byte[] output = pdf.GeneratePdf(model, builder =>
{
    builder
        .UseHtmlTemplate("templates/Invoice.html")
        .AddText("CustomerName", m => m.Name)
        .AddNumeric("Total", m => m.Total);
});
```

---

# ⚙️ Requirements

- .NET 6 or later  
- iText 8  
- BouncyCastle Adapter  
- A fillable PDF AcroForm template (for FormPdfGenerator)
- A fillable Fluid HTML template and CSS (for FluidHtmlPdfGenerator)

---

# 📄 License

MIT License for this library.

✔ iText itself is AGPL unless you have a commercial license.  

---

# 🤝 Contributing

1. Fork the repository  
2. Create a feature branch  
3. Add tests  
4. Submit a PR  

---

# 💬 Support

Open an issue if you need help with:

- Custom placeholders  
- Checkbox, radio, dropdown binding  
- HTML template rendering  
- Multi-page PDF generation  
- Export pipelines  

