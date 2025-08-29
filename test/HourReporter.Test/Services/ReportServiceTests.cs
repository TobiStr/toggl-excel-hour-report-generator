using HourReporter.Models;
using HourReporter.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace HourReporter.Test.Services;

public class ReportServiceTests
{
    private readonly Mock<ITogglApiService> _mockTogglApiService;
    private readonly Mock<IExcelReportService> _mockExcelReportService;
    private readonly Mock<ILogger<ReportService>> _mockLogger;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        _mockTogglApiService = new Mock<ITogglApiService>();
        _mockExcelReportService = new Mock<IExcelReportService>();
        _mockLogger = new Mock<ILogger<ReportService>>();
        _reportService = new ReportService(_mockTogglApiService.Object, _mockExcelReportService.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GenerateHourReportAsync_WithValidRequest_ReturnsReportData()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            ProjectName = "TestProject"
        };

        var timeEntries = new[]
        {
            new TimeEntry
            {
                Id = 1,
                Description = "Test task",
                Start = DateTime.Parse("2024-01-01T09:00:00"),
                Stop = DateTime.Parse("2024-01-01T10:00:00"),
                Duration = 3600,
                ProjectName = "TestProject"
            }
        };

        var expectedExcelData = new byte[] { 1, 2, 3, 4, 5 };

        _mockTogglApiService
            .Setup(x => x.GetTimeEntriesAsync(request.StartDate, request.EndDate, request.ProjectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeEntries);

        _mockExcelReportService
            .Setup(x => x.GenerateReportAsync(It.IsAny<HourReportData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedExcelData);

        // Act
        var result = await _reportService.GenerateHourReportAsync(request);

        // Assert
        Assert.Equal(expectedExcelData, result.ExcelData);
        Assert.Contains("TestProject", result.FileName);
        Assert.Contains("January", result.FileName);
        Assert.Contains("2024", result.FileName);
    }

    [Fact]
    public async Task GenerateHourReportAsync_WithInvalidRequest_ThrowsArgumentException()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-31"),
            EndDate = DateTime.Parse("2024-01-01"), // Invalid: start after end
            ProjectName = "TestProject"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _reportService.GenerateHourReportAsync(request));
    }

    [Fact]
    public async Task GenerateHourReportAsync_WithNoTimeEntries_ReturnsEmptyReport()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            ProjectName = "TestProject"
        };

        var expectedExcelData = new byte[] { 1, 2, 3, 4, 5 };

        _mockTogglApiService
            .Setup(x => x.GetTimeEntriesAsync(request.StartDate, request.EndDate, request.ProjectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TimeEntry>());

        _mockExcelReportService
            .Setup(x => x.GenerateReportAsync(It.IsAny<HourReportData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedExcelData);

        // Act
        var result = await _reportService.GenerateHourReportAsync(request);

        // Assert
        Assert.Equal(expectedExcelData, result.ExcelData);
        _mockExcelReportService.Verify(x => x.GenerateReportAsync(
            It.Is<HourReportData>(data => data.Rows.Count == 0 && data.TotalHours == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GenerateHourReportAsync_FiltersOutIncompleteEntries()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            ProjectName = "TestProject"
        };

        var timeEntries = new[]
        {
            new TimeEntry
            {
                Id = 1,
                Description = "Complete task",
                Start = DateTime.Parse("2024-01-01T09:00:00"),
                Stop = DateTime.Parse("2024-01-01T10:00:00"),
                Duration = 3600,
                ProjectName = "TestProject"
            },
            new TimeEntry
            {
                Id = 2,
                Description = "Incomplete task",
                Start = DateTime.Parse("2024-01-01T11:00:00"),
                Duration = -1, // Running timer
                ProjectName = "TestProject"
            }
        };

        var expectedExcelData = new byte[] { 1, 2, 3, 4, 5 };

        _mockTogglApiService
            .Setup(x => x.GetTimeEntriesAsync(request.StartDate, request.EndDate, request.ProjectName, It.IsAny<CancellationToken>()))
            .ReturnsAsync(timeEntries);

        _mockExcelReportService
            .Setup(x => x.GenerateReportAsync(It.IsAny<HourReportData>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedExcelData);

        // Act
        var result = await _reportService.GenerateHourReportAsync(request);

        // Assert
        _mockExcelReportService.Verify(x => x.GenerateReportAsync(
            It.Is<HourReportData>(data => data.Rows.Count == 1), // Only complete entries
            It.IsAny<CancellationToken>()), Times.Once);
    }
}