namespace HourReporter.Models;

public class HourReportData
{
    public List<HourReportRow> Rows { get; set; } = new();
    public string ProjectName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public double TotalHours { get; set; }

    public string GetFileName()
    {
        var monthName = StartDate.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
        var year = StartDate.Year;
        var sanitizedName = SanitizeFileName(ProjectName);
        return $"HourReport_{sanitizedName}_{monthName}_{year}.xlsx";
    }

    private static string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}

public class HourReportRow
{
    public DateTime Date { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Duration { get; set; }
    public string Tags { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
}