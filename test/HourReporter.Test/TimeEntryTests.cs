using HourReporter.Models;

namespace HourReporter.Test;

public class TimeEntryTests
{
    [Fact]
    public void DurationInHours_WithPositiveDuration_ReturnsCorrectValue()
    {
        // Arrange
        var timeEntry = new TimeEntry
        {
            Duration = 3600 // 1 hour in seconds
        };

        // Act
        var result = timeEntry.DurationInHours;

        // Assert
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void DurationInHours_WithNegativeDuration_ReturnsZero()
    {
        // Arrange
        var timeEntry = new TimeEntry
        {
            Duration = -1 // Running timer
        };

        // Act
        var result = timeEntry.DurationInHours;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DurationInHours_WithZeroDuration_ReturnsZero()
    {
        // Arrange
        var timeEntry = new TimeEntry
        {
            Duration = 0
        };

        // Act
        var result = timeEntry.DurationInHours;

        // Assert
        Assert.Equal(0, result);
    }

    [Fact]
    public void DurationInHours_WithPartialHour_ReturnsDecimalValue()
    {
        // Arrange
        var timeEntry = new TimeEntry
        {
            Duration = 1800 // 30 minutes in seconds
        };

        // Act
        var result = timeEntry.DurationInHours;

        // Assert
        Assert.Equal(0.5, result);
    }
}
