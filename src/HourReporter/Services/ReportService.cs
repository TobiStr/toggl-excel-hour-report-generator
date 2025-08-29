using HourReporter.Models;
using Microsoft.Extensions.Logging;

namespace HourReporter.Services;

public class ReportService : IReportService
{
    private readonly ITogglApiService _togglApiService;
    private readonly IExcelReportService _excelReportService;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        ITogglApiService togglApiService,
        IExcelReportService excelReportService,
        ILogger<ReportService> logger)
    {
        _togglApiService = togglApiService;
        _excelReportService = excelReportService;
        _logger = logger;
    }

    public async Task<(byte[] ExcelData, string FileName)> GenerateHourReportAsync(ReportRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting report generation for period {StartDate} to {EndDate}",
                request.StartDate, request.EndDate);

            if (!request.IsValid())
            {
                throw new ArgumentException("Invalid report request parameters");
            }

            // Fetch time entries from Toggl API
            var timeEntries = await _togglApiService.GetTimeEntriesAsync(
                request.StartDate,
                request.EndDate,
                request.ProjectName,
                cancellationToken);

            if (timeEntries.Length == 0)
            {
                _logger.LogWarning("No time entries found for the given criteria");
            }

            // Transform data for Excel report
            var reportData = TransformTimeEntriesToReportData(timeEntries, request);

            // Generate Excel file
            var excelData = await _excelReportService.GenerateReportAsync(reportData, cancellationToken);
            var fileName = reportData.GetFileName();

            _logger.LogInformation("Report generated successfully. File: {FileName}, Rows: {RowCount}, Total Hours: {TotalHours}",
                fileName, reportData.Rows.Count, reportData.TotalHours);

            return (excelData, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating hour report");
            throw;
        }
    }

    private static HourReportData TransformTimeEntriesToReportData(TimeEntry[] timeEntries, ReportRequest request)
    {
        var reportRows = timeEntries
            .Where(entry => entry.Duration > 0) // Only include completed entries
            .OrderBy(entry => entry.Start)
            .Select(entry => new HourReportRow
            {
                Date = entry.Start.Date,
                ProjectName = entry.ProjectName ?? "No Project",
                Description = entry.Description ?? "No Description",
                Duration = entry.DurationInHours,
                Tags = entry.Tags != null && entry.Tags.Length > 0 ? string.Join(", ", entry.Tags) : "",
                StartTime = entry.Start.TimeOfDay,
                EndTime = entry.Stop?.TimeOfDay
            })
            .ToList();

        var totalHours = reportRows.Sum(row => row.Duration);
        var filterName = request.GetFilterDisplayName();

        return new HourReportData
        {
            Rows = reportRows,
            ProjectName = filterName,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalHours = totalHours
        };
    }
}