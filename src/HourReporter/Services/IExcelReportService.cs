using HourReporter.Models;

namespace HourReporter.Services;

public interface IExcelReportService
{
    Task<byte[]> GenerateReportAsync(HourReportData reportData, CancellationToken cancellationToken = default);
}