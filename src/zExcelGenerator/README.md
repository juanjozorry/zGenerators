# 📘 zExcelGenerator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![NuGet](https://img.shields.io/nuget/v/zExcelGenerator?color=blue)
![NuGet (pre)](https://img.shields.io/nuget/vpre/zExcelGenerator?label=nuget-pre&color=orange)

A lightweight **fluent API** for generating Excel reports using **ClosedXML**.  
Designed for clean, expressive, multi-sheet Excel report generation with support for simple and advanced column mappings.

---

## 📑 Table of Contents

- [Features](#-features)
- [Installation](#-installation)
- [Quick Start](#-quick-start)
- [Usage Examples](#-usage-examples)
  - [Simple Columns](#simple-columns)
  - [Multiple Worksheets](#multiple-worksheets)
  - [Multiple Columns](#multiple-columns)
  - [Two Columns Per Field](#two-columns-per-field)
  - [Async & CancellationToken](#async--cancellationtoken)
- [Returning Excel from ASP.NET Core](#-returning-excel-from-aspnet-core)
- [License](#-license)

---

## ✨ Features

- 🚀 Fluent API for building Excel reports  
- 📄 Multi-worksheet support  
- 🔢 Simple and advanced column mappings  
- 🧩 Multiple-column and paired-column expansions  
- ⚙️ Works with `ILogger<T>`  
- ⏳ Supports `CancellationToken`  
- 🌐 Compatible with `netstandard2.1`  

---

## 📦 Installation

```bash
dotnet add package zExcelGenerator
```

---

## 🚀 Quick Start

```csharp
var logger = new NullLogger<ExcelGenerator>();
var generator = new ExcelGenerator(logger);

byte[] excelBytes = generator.GenerateExcel(workbook =>
{
    workbook.AddWorksheet("People", people, ws => ws
        .Column("Name", p => p.Name, 1)
        .Column("Age",  p => p.Age,  2)
        .Column("Birth Date", p => p.BirthDate, 3, "dd/MM/yyyy")
    );
});
```

---

## 📚 Usage Examples

### Simple Columns

```csharp
workbook.AddWorksheet("Products", products, ws => ws
    .Column("Id",   p => p.Id,   1)
    .Column("Name", p => p.Name, 2)
    .Column("Price", p => p.Price, 3, "#,##0.00")
);
```

### Output

| Id | Name     | Price   |
|----|----------|---------|
| 1  | Monitor  | 199.00  |
| 2  | Keyboard | 59.99   |

---

### Multiple Worksheets

```csharp
workbook
    .AddWorksheet("People", people, ws => ws
        .Column("Name", p => p.Name, 1)
        .Column("Age",  p => p.Age,  2))
    .AddWorksheet("Orders", orders, ws => ws
        .Column("Order #", o => o.OrderNumber, 1)
        .Column("Total",   o => o.TotalAmount, 2, "#,##0.00"));
```

---

### Multiple Columns

```csharp
workbook.AddWorksheet("Survey", surveyResults, ws => ws
    .Column("Name", r => r.Name, 1)
    .MultipleColumns(
        description:  "Score",
        selector:     r => r.Scores.Cast<object>(),
        totalColumns: 3,
        order: 2,
        headerSuffix: new[] { "Q1", "Q2", "Q3" },
        format: "0")
);
```

### Output Example

| Name | Score Q1 | Score Q2 | Score Q3 |
|------|----------|----------|----------|
| Ana  | 5        | 4        | 3        |

---

### Two Columns Per Field

```csharp
workbook.AddWorksheet("Monthly", monthlyData, ws => ws
    .TwoColumnsPerField(
        firstDescription:  "Planned",
        secondDescription: "Actual",
        firstSelector:     d => d.Planned.Cast<object>(),
        secondSelector:    d => d.Actual.Cast<object>(),
        totalColumns:      12,
        order: 1,
        firstHeaderSuffix:  new[] { "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" },
        secondHeaderSuffix: new[] { "Jan","Feb","Mar","Apr","May","Jun","Jul","Aug","Sep","Oct","Nov","Dec" },
        firstFormat:  "#,##0.00",
        secondFormat: "#,##0.00")
);
```

---

### Async & CancellationToken

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));

byte[] excelBytes = await generator.GenerateExcelAsync(workbook =>
{
    workbook.AddWorksheet("People", people, ws => ws
        .Column("Name", p => p.Name, 1)
        .Column("Age",  p => p.Age,  2)
    );
}, cts.Token);
```

---

## Returning Excel from ASP.NET Core

```csharp
return File(
    excelBytes,
    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    "report.xlsx"
);
```

---

## License

This project is licensed under the **MIT License**.  
See the [LICENSE](LICENSE) file for details.
