using ClosedXML.Excel;
using Xunit.Abstractions;

namespace zExcelGenerator.Tests
{
    public class ExcelGeneratorTests(ITestOutputHelper testOutputHelper)
    {
        private ExcelGenerator CreateGenerator()
        {
            var logger = new XunitLogger<ExcelGenerator>(testOutputHelper);
            return new ExcelGenerator(logger);
        }

        private XLWorkbook LoadWorkbook(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            return new XLWorkbook(ms);
        }

        // Simple test models
        private class Person
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
            public DateTime BirthDate { get; set; }
        }

        private class SurveyResult
        {
            public string Name { get; set; } = string.Empty;
            public List<int> Scores { get; set; } = new();
        }

        private class MonthlyData
        {
            public List<decimal> Planned { get; set; } = new();
            public List<decimal> Actual { get; set; } = new();
        }

        private class TemplateModel
        {
            public string CompanyName { get; set; } = string.Empty;
        }

        private class Invoice
        {
            public List<InvoiceLine> Lines { get; set; } = new();
        }

        private class InvoiceLine
        {
            public string Sku { get; set; } = string.Empty;
            public int Qty { get; set; }
        }

        private string SaveTemplate(Action<XLWorkbook> configure)
        {
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.xlsx");
            using var workbook = new XLWorkbook();
            configure(workbook);
            workbook.SaveAs(path);
            return path;
        }

        [Fact]
        public void GenerateExcel_SingleWorksheet_WithSimpleColumns_WritesHeadersAndData()
        {
            // Arrange
            var generator = CreateGenerator();

            var people = new List<Person>
            {
                new Person { Name = "Alice", Age = 30, BirthDate = new DateTime(1995, 1, 1) },
                new Person { Name = "Bob",   Age = 40, BirthDate = new DateTime(1985, 5, 10) }
            };

            // Act
            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("People", people, ws => ws
                    .Column("Name", p => p.Name, order: 1, format: "@")
                    .Column("Age", p => p.Age, order: 2, format: "0")
                    .Column("BirthDate", p => p.BirthDate, order: 3, format: "dd/MM/yyyy")
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("People");

            // Assert - headers
            Assert.Equal("Name", ws.Cell(1, 1).GetString());
            Assert.Equal("Age", ws.Cell(1, 2).GetString());
            Assert.Equal("BirthDate", ws.Cell(1, 3).GetString());

            // Assert - first data row
            Assert.Equal("Alice", ws.Cell(2, 1).GetString());
            Assert.Equal(30, ws.Cell(2, 2).GetValue<int>());
            Assert.Equal(new DateTime(1995, 1, 1), ws.Cell(2, 3).GetDateTime());

            // Assert - second data row
            Assert.Equal("Bob", ws.Cell(3, 1).GetString());
            Assert.Equal(40, ws.Cell(3, 2).GetValue<int>());
            Assert.Equal(new DateTime(1985, 5, 10), ws.Cell(3, 3).GetDateTime());
        }

        [Fact]
        public void GenerateExcel_MultipleWorksheets_CreatesAllSheets()
        {
            // Arrange
            var generator = CreateGenerator();

            var people = new List<Person>
            {
                new Person { Name = "Alice", Age = 30, BirthDate = new DateTime(1995, 1, 1) }
            };

            var surveyResults = new List<SurveyResult>
            {
                new SurveyResult { Name = "Alice", Scores = new List<int> { 5, 4, 3 } },
                new SurveyResult { Name = "Bob",   Scores = new List<int> { 3, 3, 4 } }
            };

            // Act
            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook
                    .AddWorksheet("People", people, ws => ws
                        .Column("Name", p => p.Name, 1)
                        .Column("Age", p => p.Age, 2))
                    .AddWorksheet("Survey", surveyResults, ws => ws
                        .Column("Name", r => r.Name, 1)
                        .MultipleColumns(
                            description: "Score",
                            selector: r => r.Scores.Cast<object>(),
                            totalColumns: 3,
                            order: 2,
                            headerSuffix: new[] { "Q1", "Q2", "Q3" },
                            format: "0")
                    );
            });

            using var workbookLoaded = LoadWorkbook(bytes);

            // Assert - both worksheets exist?
            Assert.NotNull(workbookLoaded.Worksheet("People"));
            Assert.NotNull(workbookLoaded.Worksheet("Survey"));

            var surveySheet = workbookLoaded.Worksheet("Survey");

            // Assert - headers for scores worksheet
            Assert.Equal("Name", surveySheet.Cell(1, 1).GetString());
            Assert.Equal("Score Q1", surveySheet.Cell(1, 2).GetString());
            Assert.Equal("Score Q2", surveySheet.Cell(1, 3).GetString());
            Assert.Equal("Score Q3", surveySheet.Cell(1, 4).GetString());

            // Assert - data for the first response
            Assert.Equal("Alice", surveySheet.Cell(2, 1).GetString());
            Assert.Equal(5, surveySheet.Cell(2, 2).GetValue<int>());
            Assert.Equal(4, surveySheet.Cell(2, 3).GetValue<int>());
            Assert.Equal(3, surveySheet.Cell(2, 4).GetValue<int>());
        }

        [Fact]
        public void GenerateExcel_MultipleColumnsMapper_CreatesExpandedColumns()
        {
            // Arrange
            var generator = CreateGenerator();

            var surveyResults = new List<SurveyResult>
            {
                new SurveyResult { Name = "Alice", Scores = new List<int> { 10, 20 } }
            };

            // Act
            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("Survey", surveyResults, ws => ws
                    .Column("Name", r => r.Name, 1)
                    .MultipleColumns(
                        description: "Score",
                        selector: r => r.Scores.Cast<object>(),
                        totalColumns: 2,
                        order: 2,
                        headerSuffix: new[] { "First", "Second" },
                        format: "0")
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("Survey");

            // Assert - headers
            Assert.Equal("Name", ws.Cell(1, 1).GetString());
            Assert.Equal("Score First", ws.Cell(1, 2).GetString());
            Assert.Equal("Score Second", ws.Cell(1, 3).GetString());

            // Assert - data
            Assert.Equal("Alice", ws.Cell(2, 1).GetString());
            Assert.Equal(10, ws.Cell(2, 2).GetValue<int>());
            Assert.Equal(20, ws.Cell(2, 3).GetValue<int>());
        }

        [Fact]
        public void GenerateExcel_TwoColumnsPerField_CreatesPairedColumns()
        {
            // Arrange
            var generator = CreateGenerator();

            var monthlyData = new List<MonthlyData>
            {
                new MonthlyData
                {
                    Planned = new List<decimal> { 100m, 200m },
                    Actual  = new List<decimal> {  90m, 210m }
                }
            };

            // Act
            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("Monthly", monthlyData, ws => ws
                    .TwoColumnsPerField(
                        firstDescription: "Planned",
                        secondDescription: "Actual",
                        firstSelector: d => d.Planned.Cast<object>(),
                        secondSelector: d => d.Actual.Cast<object>(),
                        totalColumns: 2,
                        order: 1,
                        firstHeaderSuffix: new[] { "Jan", "Feb" },
                        secondHeaderSuffix: new[] { "Jan", "Feb" },
                        firstFormat: "#,##0.00",
                        secondFormat: "#,##0.00")
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("Monthly");

            // Assert - headers
            Assert.Equal("Planned Jan", ws.Cell(1, 1).GetString());
            Assert.Equal("Actual Jan", ws.Cell(1, 2).GetString());
            Assert.Equal("Planned Feb", ws.Cell(1, 3).GetString());
            Assert.Equal("Actual Feb", ws.Cell(1, 4).GetString());

            // Assert - data
            Assert.Equal(100m, ws.Cell(2, 1).GetValue<decimal>());
            Assert.Equal(90m, ws.Cell(2, 2).GetValue<decimal>());
            Assert.Equal(200m, ws.Cell(2, 3).GetValue<decimal>());
            Assert.Equal(210m, ws.Cell(2, 4).GetValue<decimal>());
        }

        [Fact]
        public void GenerateExcel_CancellationRequested_ThrowsOperationCanceledException()
        {
            // Arrange
            var generator = CreateGenerator();

            var people = Enumerable.Range(1, 1000)
                .Select(i => new Person
                {
                    Name = $"Person {i}",
                    Age = 20 + (i % 30),
                    BirthDate = new DateTime(1990, 1, 1).AddDays(i)
                })
                .ToList();

            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel before starting to force the exception

            // Act & Assert
            Assert.Throws<OperationCanceledException>(() =>
            {
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", people, ws => ws
                        .Column("Name", p => p.Name, 1)
                        .Column("Age", p => p.Age, 2));
                }, cts.Token);
            });
        }

        [Fact]
        public void GenerateExcel_MissingMultipleColumnData_LeavesEmptyCells()
        {
            var generator = CreateGenerator();

            var surveyResults = new List<SurveyResult>
            {
                new SurveyResult { Name = "Alice", Scores = new List<int> { 10 } }
            };

            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("Survey", surveyResults, ws => ws
                    .Column("Name", r => r.Name, 1)
                    .MultipleColumns(
                        description: "Score",
                        selector: r => r.Scores.Cast<object>(),
                        totalColumns: 3,
                        order: 2,
                        headerSuffix: new[] { "Q1", "Q2", "Q3" },
                        format: "0")
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("Survey");

            Assert.Equal("Alice", ws.Cell(2, 1).GetString());
            Assert.Equal(10, ws.Cell(2, 2).GetValue<int>());
            Assert.True(ws.Cell(2, 3).IsEmpty());
            Assert.True(ws.Cell(2, 4).IsEmpty());
        }

        [Fact]
        public void GenerateExcel_AppliesNumberFormats()
        {
            var generator = CreateGenerator();

            var people = new List<Person>
            {
                new Person { Name = "Format", Age = 20, BirthDate = new DateTime(2000, 1, 1) }
            };

            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("People", people, ws => ws
                    .Column("Name", p => p.Name, 1, format: "@")
                    .Column("Age", p => p.Age, 2, format: "0")
                    .Column("BirthDate", p => p.BirthDate, 3, format: "dd/MM/yyyy")
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("People");

            Assert.Equal("@", ws.Cell(2, 1).Style.NumberFormat.Format);
            Assert.Equal("0", ws.Cell(2, 2).Style.NumberFormat.Format);
            Assert.Equal("dd/MM/yyyy", ws.Cell(2, 3).Style.NumberFormat.Format);
        }

        [Fact]
        public async Task GenerateExcelAsync_ReturnsWorkbookBytes()
        {
            var generator = CreateGenerator();

            var bytes = await generator.GenerateExcelAsync(workbook =>
            {
                workbook.AddWorksheet("People", new List<Person>
                {
                    new Person { Name = "Async", Age = 25, BirthDate = new DateTime(1999, 1, 1) }
                }, ws => ws
                    .Column("Name", p => p.Name, 1)
                    .Column("Age", p => p.Age, 2)
                    .Column("BirthDate", p => p.BirthDate, 3, format: "dd/MM/yyyy"));
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("People");

            Assert.Equal("Async", ws.Cell(2, 1).GetString());
            Assert.Equal(25, ws.Cell(2, 2).GetValue<int>());
            Assert.Equal(new DateTime(1999, 1, 1), ws.Cell(2, 3).GetDateTime());
        }

        [Fact]
        public async Task GenerateExcelAsStreamAsync_ReturnsReadableStream()
        {
            var generator = CreateGenerator();

            await using var stream = await generator.GenerateExcelAsStreamAsync(workbook =>
            {
                workbook.AddWorksheet("People", new List<Person>
                {
                    new Person { Name = "Stream", Age = 31, BirthDate = new DateTime(1993, 6, 1) }
                }, ws => ws
                    .Column("Name", p => p.Name, 1)
                    .Column("Age", p => p.Age, 2)
                    .Column("BirthDate", p => p.BirthDate, 3, format: "dd/MM/yyyy"));
            });

            Assert.True(stream.Length > 0);

            stream.Position = 0;
            using var workbookLoaded = new XLWorkbook(stream);
            var ws = workbookLoaded.Worksheet("People");

            Assert.Equal("Stream", ws.Cell(2, 1).GetString());
            Assert.Equal(31, ws.Cell(2, 2).GetValue<int>());
            Assert.Equal(new DateTime(1993, 6, 1), ws.Cell(2, 3).GetDateTime());
        }

        [Fact]
        public void GenerateExcelAsStream_ReturnsStreamAtStart()
        {
            var generator = CreateGenerator();

            using var stream = generator.GenerateExcelAsStream(workbook =>
            {
                workbook.AddWorksheet("People", new List<Person>
                {
                    new Person { Name = "Stream", Age = 31, BirthDate = new DateTime(1993, 6, 1) }
                }, ws => ws
                    .Column("Name", p => p.Name, 1)
                    .Column("Age", p => p.Age, 2)
                    .Column("BirthDate", p => p.BirthDate, 3, format: "dd/MM/yyyy"));
            });

            Assert.True(stream.CanRead);
            Assert.Equal(0, stream.Position);
            Assert.True(stream.Length > 0);
        }

        [Fact]
        public async Task GenerateExcelAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            var generator = CreateGenerator();

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                generator.GenerateExcelAsync(workbook =>
                {
                    workbook.AddWorksheet("People", new List<Person>
                    {
                        new Person { Name = "Cancelled", Age = 1, BirthDate = DateTime.Today }
                    }, ws => ws
                        .Column("Name", p => p.Name, 1)
                        .Column("Age", p => p.Age, 2)
                        .Column("BirthDate", p => p.BirthDate, 3));
                }, cts.Token));
        }

        [Fact]
        public void GenerateExcel_TwoColumnsPerField_HidesSecondColumn_WhenDisabled()
        {
            var generator = CreateGenerator();

            var monthlyData = new List<MonthlyData>
            {
                new MonthlyData
                {
                    Planned = new List<decimal> { 100m, 200m },
                    Actual  = new List<decimal> {  90m, 210m }
                }
            };

            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("Monthly", monthlyData, ws => ws
                    .TwoColumnsPerField(
                        firstDescription: "Planned",
                        secondDescription: "Actual",
                        firstSelector: d => d.Planned.Cast<object>(),
                        secondSelector: d => d.Actual.Cast<object>(),
                        totalColumns: 2,
                        order: 1,
                        firstHeaderSuffix: new[] { "Jan", "Feb" },
                        secondHeaderSuffix: new[] { "Jan", "Feb" },
                        firstFormat: "#,##0.00",
                        secondFormat: "#,##0.00",
                        showSecondColumn: false)
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("Monthly");

            Assert.Equal("Planned Jan", ws.Cell(1, 1).GetString());
            Assert.Equal("Planned Feb", ws.Cell(1, 2).GetString());

            Assert.Equal(100m, ws.Cell(2, 1).GetValue<decimal>());
            Assert.Equal(200m, ws.Cell(2, 2).GetValue<decimal>());

            Assert.True(ws.Cell(1, 3).IsEmpty());
            Assert.True(ws.Cell(2, 3).IsEmpty());
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRange_SetsValueAcrossMultiRange()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Main");
                ws.Cell("A1").Value = "Old";
                ws.Cell("C1").Value = "Old";

                workbook.DefinedNames.Add("CompanyName", "Main!$A$1,Main!$C$1");
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(model)
                    .NamedRange("CompanyName", x => x.CompanyName));

                using var workbookLoaded = LoadWorkbook(bytes);
                var ws = workbookLoaded.Worksheet("Main");

                Assert.Equal("zGenerators", ws.Cell("A1").GetString());
                Assert.Equal("zGenerators", ws.Cell("C1").GetString());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRangeTable_InsertsRowsBelowHeader()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Report");
                ws.Cell("A1").Value = "Sku";
                ws.Cell("B1").Value = "Qty";
                ws.Cell("A3").Value = "Footer";

                workbook.DefinedNames.Add("LinesHeader", "Report!$A$1");
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 },
                        new InvoiceLine { Sku = "B-02", Qty = 5 }
                    }
                };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(invoice)
                    .NamedRangeTable("LinesHeader", x => x.Lines, table => table
                        .Column("Sku", l => l.Sku, 1)
                        .Column("Qty", l => l.Qty, 2, format: "0")));

                using var workbookLoaded = LoadWorkbook(bytes);
                var ws = workbookLoaded.Worksheet("Report");

                Assert.Equal("Sku", ws.Cell("A1").GetString());
                Assert.Equal("Qty", ws.Cell("B1").GetString());

                Assert.Equal("A-01", ws.Cell("A2").GetString());
                Assert.Equal(2, ws.Cell("B2").GetValue<int>());
                Assert.Equal("B-02", ws.Cell("A3").GetString());
                Assert.Equal(5, ws.Cell("B3").GetValue<int>());

                Assert.Equal("Footer", ws.Cell("A4").GetString());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_ForWorksheet_MapsNamedRangesPerSheet()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var cover = workbook.AddWorksheet("Cover");
                var lines = workbook.AddWorksheet("Lines");

                cover.Cell("B2").Value = "Old";
                lines.Cell("B2").Value = "Old";

                cover.DefinedNames.Add("CustomerName", cover.Range("B2"));
                lines.DefinedNames.Add("CustomerName", lines.Range("B2"));
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(model)
                    .ForWorksheet("Cover", ws => ws
                        .NamedRange("CustomerName", x => x.CompanyName))
                    .ForWorksheet("Lines", ws => ws
                        .NamedRange("CustomerName", x => x.CompanyName)));

                using var workbookLoaded = LoadWorkbook(bytes);

                Assert.Equal("zGenerators", workbookLoaded.Worksheet("Cover").Cell("B2").GetString());
                Assert.Equal("zGenerators", workbookLoaded.Worksheet("Lines").Cell("B2").GetString());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_AddWorksheet_AppendsNewSheet()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var cover = workbook.AddWorksheet("Cover");
                cover.Cell("B2").Value = "Old";
                cover.DefinedNames.Add("CustomerName", cover.Range("B2"));
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };
                var lines = new List<InvoiceLine>
                {
                    new InvoiceLine { Sku = "A-01", Qty = 2 },
                    new InvoiceLine { Sku = "B-02", Qty = 5 }
                };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(model)
                    .ForWorksheet("Cover", ws => ws
                        .NamedRange("CustomerName", x => x.CompanyName))
                    .AddWorksheet("Summary", lines, ws => ws
                        .Column("Sku", l => l.Sku, 1)
                        .Column("Qty", l => l.Qty, 2, format: "0")));

                using var workbookLoaded = LoadWorkbook(bytes);
                var cover = workbookLoaded.Worksheet("Cover");
                var summary = workbookLoaded.Worksheet("Summary");

                Assert.Equal("zGenerators", cover.Cell("B2").GetString());
                Assert.Equal("Sku", summary.Cell(1, 1).GetString());
                Assert.Equal("Qty", summary.Cell(1, 2).GetString());
                Assert.Equal("A-01", summary.Cell(2, 1).GetString());
                Assert.Equal(2, summary.Cell(2, 2).GetValue<int>());
                Assert.Equal("B-02", summary.Cell(3, 1).GetString());
                Assert.Equal(5, summary.Cell(3, 2).GetValue<int>());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_ForWorksheet_ThrowsWhenNamedRangeMissingOnSheet()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var cover = workbook.AddWorksheet("Cover");
                cover.Cell("B2").Value = "Old";
                cover.DefinedNames.Add("CustomerName", cover.Range("B2"));
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };

                Assert.Throws<InvalidOperationException>(() =>
                {
                    generator.GenerateExcelFromTemplate(tpl => tpl
                        .UseTemplatePath(templatePath)
                        .SetData(model)
                        .ForWorksheet("Lines", ws => ws
                            .NamedRange("CustomerName", x => x.CompanyName)));
                });
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRangeTable_HeaderRowIsNamedRangeFalse_WritesHeadersAtNamedRangeRow()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Lines");
                ws.Cell("A4").Value = "Footer";
                ws.DefinedNames.Add("LinesData", ws.Range("A2"));
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 }
                    }
                };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(invoice)
                    .NamedRangeTable("LinesData", x => x.Lines, table => table
                        .Column("Sku", l => l.Sku, 1)
                        .Column("Qty", l => l.Qty, 2, format: "0"),
                        headerRowIsNamedRange: false,
                        writeHeaders: true,
                        insertRows: false));

                using var workbookLoaded = LoadWorkbook(bytes);
                var ws = workbookLoaded.Worksheet("Lines");

                Assert.Equal("Sku", ws.Cell("A2").GetString());
                Assert.Equal("Qty", ws.Cell("B2").GetString());
                Assert.Equal("A-01", ws.Cell("A3").GetString());
                Assert.Equal(2, ws.Cell("B3").GetValue<int>());
                Assert.Equal("Footer", ws.Cell("A4").GetString());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRangeTable_ThrowsWhenNoColumnsConfigured()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Lines");
                ws.DefinedNames.Add("LinesHeader", ws.Range("A1"));
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 }
                    }
                };

                Assert.Throws<InvalidOperationException>(() =>
                {
                    generator.GenerateExcelFromTemplate(tpl => tpl
                        .UseTemplatePath(templatePath)
                        .SetData(invoice)
                        .NamedRangeTable("LinesHeader", x => x.Lines, _ => { }));
                });
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_AddWorksheet_ThrowsWhenReportNameInvalid()
        {
            var generator = CreateGenerator();
            var model = new TemplateModel { CompanyName = "zGenerators" };

            Assert.Throws<ArgumentException>(() =>
            {
                generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath("Templates/Invoice.xlsx")
                    .SetData(model)
                    .AddWorksheet(" ", new List<InvoiceLine>(), ws => ws
                        .Column("Sku", l => l.Sku, 1)));
            });
        }

        [Fact]
        public void GenerateExcelFromTemplate_AddWorksheet_ThrowsWhenItemsNull()
        {
            var generator = CreateGenerator();
            var model = new TemplateModel { CompanyName = "zGenerators" };

            Assert.Throws<ArgumentNullException>(() =>
            {
                generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath("Templates/Invoice.xlsx")
                    .SetData(model)
                    .AddWorksheet<InvoiceLine>("Summary", null!, ws => ws
                        .Column("Sku", l => l.Sku, 1)));
            });
        }

        [Fact]
        public void GenerateExcelFromTemplate_ForWorksheet_ThrowsWhenWorksheetNameInvalid()
        {
            var generator = CreateGenerator();
            var model = new TemplateModel { CompanyName = "zGenerators" };

            Assert.Throws<ArgumentException>(() =>
            {
                generator.GenerateExcelFromTemplate(tpl => tpl
                    .SetData(model)
                    .ForWorksheet(" ", ws => ws
                        .NamedRange("CustomerName", x => x.CompanyName)));
            });
        }

        [Fact]
        public void GenerateExcelFromTemplate_ThrowsWhenTemplatePathMissing()
        {
            var generator = CreateGenerator();
            var model = new TemplateModel { CompanyName = "zGenerators" };

            Assert.Throws<InvalidOperationException>(() =>
            {
                generator.GenerateExcelFromTemplate(tpl => tpl
                    .SetData(model)
                    .NamedRange("CustomerName", x => x.CompanyName));
            });
        }

        [Fact]
        public void GenerateExcelFromTemplate_ThrowsWhenModelMissing()
        {
            var generator = CreateGenerator();

            Assert.Throws<InvalidOperationException>(() =>
            {
                generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath("Templates/Invoice.xlsx"));
            });
        }

        [Fact]
        public void GenerateExcelFromTemplate_ThrowsWhenNamedRangeMissing()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                workbook.AddWorksheet("Cover");
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };

                Assert.Throws<InvalidOperationException>(() =>
                {
                    generator.GenerateExcelFromTemplate(tpl => tpl
                        .UseTemplatePath(templatePath)
                        .SetData(model)
                        .NamedRange("CustomerName", x => x.CompanyName));
                });
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_ThrowsWhenNamedRangeTableMissing()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                workbook.AddWorksheet("Lines");
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 }
                    }
                };

                Assert.Throws<InvalidOperationException>(() =>
                {
                    generator.GenerateExcelFromTemplate(tpl => tpl
                        .UseTemplatePath(templatePath)
                        .SetData(invoice)
                        .NamedRangeTable("LinesHeader", x => x.Lines, table => table
                            .Column("Sku", l => l.Sku, 1)));
                });
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_ForWorksheet_ThrowsWhenWorksheetNamedRangeExistsInOtherSheet()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var cover = workbook.AddWorksheet("Cover");
                cover.Cell("B2").Value = "Old";
                cover.DefinedNames.Add("CustomerName", cover.Range("B2"));
                workbook.AddWorksheet("Lines");
            });

            try
            {
                var model = new TemplateModel { CompanyName = "zGenerators" };

                Assert.Throws<InvalidOperationException>(() =>
                {
                    generator.GenerateExcelFromTemplate(tpl => tpl
                        .UseTemplatePath(templatePath)
                        .SetData(model)
                        .ForWorksheet("Lines", ws => ws
                            .NamedRange("CustomerName", x => x.CompanyName)));
                });
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRangeTable_InsertRowsFalse_DoesNotShiftFooter()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Lines");
                ws.Cell("A1").Value = "Sku";
                ws.Cell("B1").Value = "Qty";
                ws.Cell("A3").Value = "Footer";
                workbook.DefinedNames.Add("LinesHeader", ws.Range("A1"));
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 },
                        new InvoiceLine { Sku = "B-02", Qty = 5 }
                    }
                };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(invoice)
                    .NamedRangeTable("LinesHeader", x => x.Lines, table => table
                        .Column("Sku", l => l.Sku, 1)
                        .Column("Qty", l => l.Qty, 2, format: "0"),
                        headerRowIsNamedRange: true,
                        writeHeaders: false,
                        insertRows: false));

                using var workbookLoaded = LoadWorkbook(bytes);
                var ws = workbookLoaded.Worksheet("Lines");

                Assert.Equal("A-01", ws.Cell("A2").GetString());
                Assert.Equal(2, ws.Cell("B2").GetValue<int>());
                Assert.Equal("B-02", ws.Cell("A3").GetString());
                Assert.Equal(5, ws.Cell("B3").GetValue<int>());
                Assert.NotEqual("Footer", ws.Cell("A3").GetString());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcelFromTemplate_NamedRangeTable_WriteHeadersFalse_DoesNotOverwriteTemplateHeaders()
        {
            var generator = CreateGenerator();

            var templatePath = SaveTemplate(workbook =>
            {
                var ws = workbook.AddWorksheet("Lines");
                ws.Cell("A1").Value = "TemplateSku";
                ws.Cell("B1").Value = "TemplateQty";
                workbook.DefinedNames.Add("LinesHeader", ws.Range("A1"));
            });

            try
            {
                var invoice = new Invoice
                {
                    Lines = new List<InvoiceLine>
                    {
                        new InvoiceLine { Sku = "A-01", Qty = 2 }
                    }
                };

                var bytes = generator.GenerateExcelFromTemplate(tpl => tpl
                    .UseTemplatePath(templatePath)
                    .SetData(invoice)
                    .NamedRangeTable("LinesHeader", x => x.Lines, table => table
                        .Column("Sku", l => l.Sku, 1)
                        .Column("Qty", l => l.Qty, 2, format: "0"),
                        headerRowIsNamedRange: true,
                        writeHeaders: false,
                        insertRows: true));

                using var workbookLoaded = LoadWorkbook(bytes);
                var ws = workbookLoaded.Worksheet("Lines");

                Assert.Equal("TemplateSku", ws.Cell("A1").GetString());
                Assert.Equal("TemplateQty", ws.Cell("B1").GetString());
                Assert.Equal("A-01", ws.Cell("A2").GetString());
                Assert.Equal(2, ws.Cell("B2").GetValue<int>());
            }
            finally
            {
                if (File.Exists(templatePath))
                {
                    File.Delete(templatePath);
                }
            }
        }

        [Fact]
        public void GenerateExcel_Cancels_MidExecution()
        {
            var generator = CreateGenerator();

            var people = new List<Person>
            {
                new Person { Name = "First", Age = 20, BirthDate = new DateTime(2000, 1, 1) },
                new Person { Name = "Second", Age = 21, BirthDate = new DateTime(2001, 1, 1) }
            };

            using var cts = new CancellationTokenSource();

            Assert.Throws<OperationCanceledException>(() =>
            {
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", people, ws => ws
                        .Column("Name", p =>
                        {
                            cts.Cancel();
                            return p.Name;
                        }, 1)
                        .Column("Age", p => p.Age, 2));
                }, cts.Token);
            });
        }

        [Fact]
        public void AddWorksheet_Throws_WhenReportNameInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet<Person>(" ", new List<Person>(), ws => ws
                        .Column("Name", p => p.Name, 1));
                }));
        }

        [Fact]
        public void AddWorksheet_Throws_WhenItemsNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet<Person>("People", null!, ws => ws
                        .Column("Name", p => p.Name, 1));
                }));
        }

        [Fact]
        public void AddWorksheet_Throws_WhenConfigureColumnsNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet<Person>("People", new List<Person>(), configureColumns: null!);
                }));
        }

        [Fact]
        public void AddWorksheet_Throws_WhenNoColumnsConfigured()
        {
            var generator = CreateGenerator();

            Assert.Throws<InvalidOperationException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", new List<Person>(), ws => { });
                }));
        }

        [Fact]
        public void WorksheetBuilder_Column_Throws_WhenDescriptionInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", new List<Person>(), ws =>
                        ws.Column(" ", p => p.Name, 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_Column_Throws_WhenSelectorNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", new List<Person>(), ws =>
                        ws.Column("Name", null!, 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_MultipleColumns_Throws_WhenTotalColumnsInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Survey", new List<SurveyResult>(), ws =>
                        ws.MultipleColumns("Score", r => r.Scores.Cast<object>(), 0, 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_TwoColumnsPerField_Throws_WhenTotalColumnsInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Monthly", new List<MonthlyData>(), ws =>
                        ws.TwoColumnsPerField(
                            firstDescription: "Planned",
                            secondDescription: "Actual",
                            firstSelector: d => d.Planned.Cast<object>(),
                            secondSelector: d => d.Actual.Cast<object>(),
                            totalColumns: 0,
                            order: 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_Mapper_Throws_WhenMapperNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("People", new List<Person>(), ws =>
                        ws.Mapper(null!));
                }));
        }

        [Fact]
        public void WorksheetBuilder_MultipleColumns_Throws_WhenDescriptionInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Survey", new List<SurveyResult>(), ws =>
                        ws.MultipleColumns(" ", r => r.Scores.Cast<object>(), 1, 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_TwoColumnsPerField_Throws_WhenDescriptionsInvalid()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Monthly", new List<MonthlyData>(), ws =>
                        ws.TwoColumnsPerField(
                            firstDescription: " ",
                            secondDescription: "Actual",
                            firstSelector: d => d.Planned.Cast<object>(),
                            secondSelector: d => d.Actual.Cast<object>(),
                            totalColumns: 1,
                            order: 1));
                }));

            Assert.Throws<ArgumentException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Monthly", new List<MonthlyData>(), ws =>
                        ws.TwoColumnsPerField(
                            firstDescription: "Planned",
                            secondDescription: " ",
                            firstSelector: d => d.Planned.Cast<object>(),
                            secondSelector: d => d.Actual.Cast<object>(),
                            totalColumns: 1,
                            order: 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_Column_StoresFormatAndAlignment()
        {
            var generator = CreateGenerator();

            var bytes = generator.GenerateExcel(workbook =>
            {
                workbook.AddWorksheet("People", new List<Person>
                {
                    new Person { Name = "Align", Age = 20, BirthDate = new DateTime(2000, 1, 1) }
                }, ws => ws
                    .Column("Name", p => p.Name, 1, format: "@", alignment: XLAlignmentHorizontalValues.Right)
                );
            });

            using var workbookLoaded = LoadWorkbook(bytes);
            var ws = workbookLoaded.Worksheet("People");

            Assert.Equal("@", ws.Cell(2, 1).Style.NumberFormat.Format);
            Assert.Equal(XLAlignmentHorizontalValues.Right, ws.Cell(2, 1).Style.Alignment.Horizontal);
        }

        [Fact]
        public void WorksheetBuilder_MultipleColumns_Throws_WhenSelectorNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Survey", new List<SurveyResult>(), ws =>
                        ws.MultipleColumns("Score", null!, 1, 1));
                }));
        }

        [Fact]
        public void WorksheetBuilder_TwoColumnsPerField_Throws_WhenSelectorsNull()
        {
            var generator = CreateGenerator();

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Monthly", new List<MonthlyData>(), ws =>
                        ws.TwoColumnsPerField(
                            firstDescription: "Planned",
                            secondDescription: "Actual",
                            firstSelector: null!,
                            secondSelector: d => d.Actual.Cast<object>(),
                            totalColumns: 1,
                            order: 1));
                }));

            Assert.Throws<ArgumentNullException>(() =>
                generator.GenerateExcel(workbook =>
                {
                    workbook.AddWorksheet("Monthly", new List<MonthlyData>(), ws =>
                        ws.TwoColumnsPerField(
                            firstDescription: "Planned",
                            secondDescription: "Actual",
                            firstSelector: d => d.Planned.Cast<object>(),
                            secondSelector: null!,
                            totalColumns: 1,
                            order: 1));
                }));
        }
    }
}
