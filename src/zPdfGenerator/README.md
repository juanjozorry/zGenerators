# zPdfGenerator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zPdfGenerator?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zPdfGenerator?label=nuget-pre&color=orange)


A lightweight, fluent, and extensible PDF generation toolkit for .NET.
It provides two high-level generators and an extensible post-processing pipeline for advanced PDF operations.

---

## Available Generators

### 1. FormPdfGenerator (AcroForm PDF filler)

Fills existing PDF form templates (AcroForms) with strongly-typed data models using a fluent builder and placeholder system.

### 2. FluidHtmlPdfGenerator (HTML → PDF renderer)

Generates PDF documents from Fluid HTML templates and a strongly typed model.

---

## Features

### Shared features (both generators)

* Fluent API with strongly-typed placeholders
* Automatic culture-aware formatting (dates, numbers, currencies)
* Extensible architecture with post-processing pipeline
* Built-in logging via ILogger<T>
* Supports cancellation tokens
* Fully testable and mockable

---

## Installation

```bash
dotnet add package zPdfGenerator
```

### Required for FormPdfGenerator

```bash
dotnet add package itext
dotnet add package itext.forms
dotnet add package itext.bouncy-castle-adapter
```

### Required for FluidHtmlPdfGenerator

```bash
dotnet add package itext.pdfhtml
dotnet add package FluidCore
```

---

# Post-Processing Pipeline

zPdfGenerator supports a composable post-processing pipeline that operates on the generated PDF bytes.
Each post-processor implements the following interface:

```csharp
public interface IPostProcessor
{
    bool LastPostProcessor { get; }
    byte[] Process(byte[] pdfData, CancellationToken cancellationToken);
}
```

Post-processors can be chained and executed in order. A single post-processor can be marked as `LastPostProcessor` (typically the digital signature), which is always executed at the end.

---

## Available Post-Processors

### 1. DocumentClassifierPostProcessor

Adds document classification information using PDF metadata only (no visual changes).

**Use cases**:

* Allow users to see classification in Acrobat Reader (File → Properties)
* Enable downstream systems (DLP, indexing, compliance) to detect sensitivity

**Metadata written**:

* Subject (e.g. `Classification: Confidential`)
* Keywords (e.g. `classification=confidential`)
* Custom fields: `Classification`, `SI_DATA`

**Using the builder API**:

```csharp
var pdfBytes = generator.GeneratePdf(model, builder =>
{
    builder
        .UseHtmlTemplate("templates/Invoice.html")
        .AddText("CustomerName", m => m.Name)
        .AddNumeric("Total", m => m.Total)

        // Post-processing
        .AddPostDocumentClassifier(
            ClassificationEnum.Internal,
            new Dictionary<string, string>
            {
                ["Department"] = "Finance",
                ["System"] = "ERP"
            });
});
```

Reserved keys such as `Classification` and `SI_DATA` cannot be overridden.

---

### 2. PasswordProtectPostProcessor

Encrypts the PDF using a user password and an owner (master) password.

**Behavior**:

* The document cannot be opened without a password
* User password allows reading
* Owner password allows full control

**Using the builder API**:

```csharp
var pdfBytes = generator.GeneratePdf(model, builder =>
{
    builder
        .UseHtmlTemplate("templates/Invoice.html")
        .AddText("CustomerName", m => m.Name)
        .AddNumeric("Total", m => m.Total)

        // Post-processing
        .AddPostPasswordProtect(
            masterPassword: "owner-secret",
            userPassword: "user-secret");
});
```

This post-processor must be executed before digital signing.

---

### 3. PfxDigitalSignaturePostProcessor

Applies a digital signature to the PDF using a PFX (PKCS#12) certificate.

**Key points**:

* Must be the last post-processor in the pipeline
* Uses append mode to preserve previous changes
* Supports visible and invisible signatures

**Using the builder API**:

```csharp
var signatureOptions = new PdfSignatureOptions(
    pfxPassword: "secret",
    fieldName: "Signature1",
    visible: false
);

var pdfBytes = generator.GeneratePdf(model, builder =>
{
    builder
        .UseHtmlTemplate("templates/Invoice.html")
        .AddText("CustomerName", m => m.Name)
        .AddNumeric("Total", m => m.Total)

        // Classification
        .AddPostDocumentClassifier(
            ClassificationEnum.Confidential,
            new Dictionary<string, string> { ["Department"] = "Finance" })

        // Digital signature (always last)
        .AddPostPfxDigitalSignature(pfxBytes, signatureOptions);
});
```

The resulting PDF can be validated in Acrobat Reader (Signature Panel).

---

## FormPdfGenerator

Fills PDF AcroForms using a fluent builder and placeholder mapping.

### Example Model

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

### Simple Example

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
```

---

## FluidHtmlPdfGenerator

Renders PDFs from Fluid HTML templates.

### Example

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

## Requirements

* netstandard2.1 or higher
* iText 9.x
* BouncyCastle adapter
* A fillable PDF AcroForm template (FormPdfGenerator)
* A Fluid HTML template and CSS (FluidHtmlPdfGenerator)

---

## License

MIT License for this library.

iText itself is AGPL unless you have a commercial license.

---

## Contributing

1. Fork the repository
2. Create a feature branch
3. Add tests
4. Submit a PR

---

## Support

Open an issue if you need help with:

* Custom placeholders
* HTML template rendering
* Post-processing pipelines
* Digital signatures
* PDF security and compliance
