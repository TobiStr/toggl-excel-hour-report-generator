using HourReporter.Models;

namespace HourReporter.Test.Models;

public class HourReportDataTests
{
    [Fact]
    public void GetFileName_GeneratesCorrectFileName()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "Test Client",
            StartDate = DateTime.Parse("2024-01-15"),
            EndDate = DateTime.Parse("2024-01-31")
        };

        // Act
        var fileName = reportData.GetFileName();

        // Assert
        Assert.Equal("HourReport_Test Client_January_2024.xlsx", fileName);
    }

    [Fact]
    public void GetFileName_SanitizesInvalidCharacters()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "Test/Client<With>Invalid:Chars",
            StartDate = DateTime.Parse("2024-01-15"),
            EndDate = DateTime.Parse("2024-01-31")
        };

        // Act
        var fileName = reportData.GetFileName();

        // Assert
        Assert.Equal("HourReport_Test_Client_With_Invalid_Chars_January_2024.xlsx", fileName);
    }

    [Fact]
    public void GetFileName_HandlesEmptyClientName()
    {
        // Arrange
        var reportData = new HourReportData
        {
            ProjectName = "",
            StartDate = DateTime.Parse("2024-12-15"),
            EndDate = DateTime.Parse("2024-12-31")
        };

        // Act
        var fileName = reportData.GetFileName();

        // Assert
        Assert.Equal("HourReport__December_2024.xlsx", fileName);
    }
}