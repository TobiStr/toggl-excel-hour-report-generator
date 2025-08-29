using HourReporter.Models;

namespace HourReporter.Test.Models;

public class ReportRequestTests
{
    [Fact]
    public void IsValid_WithValidProjectName_ReturnsTrue()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31"),
            ProjectName = "TestProject"
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsValid_WithStartDateAfterEndDate_ReturnsFalse()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-31"),
            EndDate = DateTime.Parse("2024-01-01"),
            ProjectName = "TestProject"
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNoProject_ReturnsFalse()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-31")
        };

        // Act
        var result = request.IsValid();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetFilterDisplayName_WithProjectName_ReturnsProjectName()
    {
        // Arrange
        var request = new ReportRequest { ProjectName = "TestProject" };

        // Act
        var result = request.GetFilterDisplayName();

        // Assert
        Assert.Equal("TestProject", result);
    }

    [Fact]
    public void GetFilterDisplayName_WithNoFilter_ReturnsUnknown()
    {
        // Arrange
        var request = new ReportRequest();

        // Act
        var result = request.GetFilterDisplayName();

        // Assert
        Assert.Equal("Unknown", result);
    }
}