namespace HourReporter.Models;

public class ReportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string? ProjectName { get; set; }
    public decimal? HourlyRate { get; set; }

    public bool IsValid()
    {
        return StartDate <= EndDate &&
               StartDate != default &&
               EndDate != default &&
               !string.IsNullOrEmpty(ProjectName);
    }

    public string GetFilterDisplayName()
    {
        return ProjectName ?? "Unknown";
    }
}