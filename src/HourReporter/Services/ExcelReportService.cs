using ClosedXML.Excel;
using HourReporter.Models;
using Microsoft.Extensions.Logging;

namespace HourReporter.Services;

public class ExcelReportService : IExcelReportService
{
    private readonly ILogger<ExcelReportService> _logger;

    public ExcelReportService(ILogger<ExcelReportService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateReportAsync(HourReportData reportData, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Generating Excel report with {RowCount} rows", reportData.Rows.Count);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Report");

            await CreateHeadersAsync(worksheet, cancellationToken);
            await PopulateDataAsync(worksheet, reportData, cancellationToken);
            await FormatWorksheetAsync(worksheet, reportData.Rows.Count, cancellationToken);
            await AddSummaryAsync(worksheet, reportData, cancellationToken);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);

            _logger.LogInformation("Excel report generated successfully");
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Excel report");
            throw;
        }
    }

    private static Task CreateHeadersAsync(IXLWorksheet worksheet, CancellationToken cancellationToken)
    {
        var headers = new[]
        {
            "Date", "Project", "Description", "Duration (Hours)",
            "Tags", "Start Time", "End Time"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightBlue;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        return Task.CompletedTask;
    }

    private static Task PopulateDataAsync(IXLWorksheet worksheet, HourReportData reportData, CancellationToken cancellationToken)
    {
        for (int i = 0; i < reportData.Rows.Count; i++)
        {
            var row = reportData.Rows[i];
            var excelRow = i + 2; // Starting from row 2 (after header)

            worksheet.Cell(excelRow, 1).Value = row.Date.ToString("yyyy-MM-dd");
            worksheet.Cell(excelRow, 2).Value = row.ProjectName;
            worksheet.Cell(excelRow, 3).Value = row.Description;
            worksheet.Cell(excelRow, 4).Value = Math.Round(row.Duration, 2);
            worksheet.Cell(excelRow, 5).Value = row.Tags;
            worksheet.Cell(excelRow, 6).Value = row.StartTime.ToString(@"hh\:mm");
            worksheet.Cell(excelRow, 7).Value = row.EndTime?.ToString(@"hh\:mm") ?? "";

            // Add borders to data cells
            var rowRange = worksheet.Range(excelRow, 1, excelRow, 7);
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            // Alternate row colors
            if (i % 2 == 1)
            {
                rowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(250, 250, 250);
            }
        }

        return Task.CompletedTask;
    }

    private static Task FormatWorksheetAsync(IXLWorksheet worksheet, int rowCount, CancellationToken cancellationToken)
    {
        // Auto-fit columns
        worksheet.ColumnsUsed().AdjustToContents();

        // Set minimum column widths
        worksheet.Column(1).Width = Math.Max(worksheet.Column(1).Width, 12); // Date
        worksheet.Column(2).Width = Math.Max(worksheet.Column(2).Width, 15); // Project
        worksheet.Column(3).Width = Math.Max(worksheet.Column(3).Width, 30); // Description
        worksheet.Column(4).Width = Math.Max(worksheet.Column(4).Width, 12); // Duration
        worksheet.Column(5).Width = Math.Max(worksheet.Column(5).Width, 15); // Tags
        worksheet.Column(6).Width = Math.Max(worksheet.Column(6).Width, 10); // Start Time
        worksheet.Column(7).Width = Math.Max(worksheet.Column(7).Width, 10); // End Time

        // Format duration column as number with 2 decimal places
        if (rowCount > 0)
        {
            worksheet.Range(2, 4, rowCount + 1, 4).Style.NumberFormat.Format = "0.00";
        }

        return Task.CompletedTask;
    }

    private static Task AddSummaryAsync(IXLWorksheet worksheet, HourReportData reportData, CancellationToken cancellationToken)
    {
        var summaryRow = reportData.Rows.Count + 3; // Leave a blank row

        // Add summary information
        var titleCell = worksheet.Cell(summaryRow, 1);
        titleCell.Value = "Report Summary";
        titleCell.Style.Font.Bold = true;
        titleCell.Style.Font.FontSize = 14;

        worksheet.Cell(summaryRow + 1, 1).Value = "Period:";
        worksheet.Cell(summaryRow + 1, 2).Value = $"{reportData.StartDate:yyyy-MM-dd} to {reportData.EndDate:yyyy-MM-dd}";

        worksheet.Cell(summaryRow + 2, 1).Value = "Project:";
        worksheet.Cell(summaryRow + 2, 2).Value = reportData.ProjectName;

        worksheet.Cell(summaryRow + 3, 1).Value = "Total Hours:";
        var totalHoursCell = worksheet.Cell(summaryRow + 3, 2);
        totalHoursCell.Value = Math.Round(reportData.TotalHours, 2);
        totalHoursCell.Style.Font.Bold = true;

        worksheet.Cell(summaryRow + 4, 1).Value = "Total Entries:";
        worksheet.Cell(summaryRow + 4, 2).Value = reportData.Rows.Count;

        // Format summary section
        var summaryRange = worksheet.Range(summaryRow, 1, summaryRow + 4, 2);
        summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        summaryRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

        return Task.CompletedTask;
    }
}