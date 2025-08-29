using HourReporter.Models;

namespace HourReporter.Services;

public interface IReportService
{
    Task<(byte[] ExcelData, string FileName)> GenerateHourReportAsync(ReportRequest request, CancellationToken cancellationToken = default);
}