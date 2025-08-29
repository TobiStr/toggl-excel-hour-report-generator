using HourReporter.Models;

namespace HourReporter.Services;

public interface ITogglApiService
{
    Task<TimeEntry[]> GetTimeEntriesAsync(DateTime startDate, DateTime endDate, string? projectName = null, CancellationToken cancellationToken = default);
}