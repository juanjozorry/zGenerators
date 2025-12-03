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
            public string Name { get; set; }
            public int Age { get; set; }
            public DateTime BirthDate { get; set; }
        }

        private class SurveyResult
        {
            public string Name { get; set; }
            public List<int> Scores { get; set; } = new();
        }

        private class MonthlyData
        {
            public List<decimal> Planned { get; set; } = new();
            public List<decimal> Actual { get; set; } = new();
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
    }
}
