using HourReporter.Models;
using HourReporter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HourReporter.Test.Services;

public class ExcelReportServiceTests
{
    private readonly Mock<ILogger<ExcelReportService>> _mockLogger;
    private readonly ExcelReportService _excelReportService;

    public ExcelReportServiceTests()
    {
        _mockLogger = new Mock<ILogger<ExcelReportService>>();
        _excelReportService = new ExcelReportService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateReportAsync_WithValidData_ReturnsExcelBytes()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "TestProject",
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            TotalHours = 8.5,
            Rows = new List<HourReportRow>
            {
                new()
                {
                    Date = DateTime.Parse("2024-01-01"),
                    ProjectName = "TestProject",
                    Description = "Test task",
                    Duration = 8.5,
                    Tags = "tag1, tag2",
                    StartTime = TimeSpan.Parse("09:00:00"),
                    EndTime = TimeSpan.Parse("17:30:00")
                }
            }
        };

        // Act
        var result = await _excelReportService.GenerateReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
        
        // Verify it starts with Excel file signature (ZIP format)
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public async Task GenerateReportAsync_WithEmptyData_ReturnsValidExcel()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "TestProject",
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            TotalHours = 0,
            Rows = new List<HourReportRow>()
        };

        // Act
        var result = await _excelReportService.GenerateReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }

    [Fact]
    public async Task GenerateReportAsync_WithMultipleRows_ReturnsValidExcel()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "TestProject",
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            TotalHours = 16.0,
            Rows = new List<HourReportRow>
            {
                new()
                {
                    Date = DateTime.Parse("2024-01-01"),
                    ProjectName = "Project1",
                    Description = "Task 1",
                    Duration = 8.0,
                    Tags = "",
                    StartTime = TimeSpan.Parse("09:00:00"),
                    EndTime = TimeSpan.Parse("17:00:00")
                },
                new()
                {
                    Date = DateTime.Parse("2024-01-02"),
                    ProjectName = "Project2",
                    Description = "Task 2",
                    Duration = 8.0,
                    Tags = "urgent",
                    StartTime = TimeSpan.Parse("08:00:00"),
                    EndTime = TimeSpan.Parse("16:00:00")
                }
            }
        };

        // Act
        var result = await _excelReportService.GenerateReportAsync(reportData);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}