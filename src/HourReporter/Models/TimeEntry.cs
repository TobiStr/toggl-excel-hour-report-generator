using System.Text.Json.Serialization;

namespace HourReporter.Models;

public class TimeEntry
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("start")]
    public DateTime Start { get; set; }

    [JsonPropertyName("stop")]
    public DateTime? Stop { get; set; }

    [JsonPropertyName("duration")]
    public int Duration { get; set; }

    [JsonPropertyName("workspace_id")]
    public long? WorkspaceId { get; set; }

    [JsonPropertyName("project_id")]
    public long? ProjectId { get; set; }

    [JsonPropertyName("project_name")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("tags")]
    public string[]? Tags { get; set; }

    [JsonPropertyName("server_deleted_at")]
    public string? ServerDeletedAt { get; set; }

    public double DurationInHours => Duration > 0 ? Duration / 3600.0 : 0;
}