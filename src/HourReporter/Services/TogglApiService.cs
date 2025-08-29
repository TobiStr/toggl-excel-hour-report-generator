using System.Text;
using System.Text.Json;
using HourReporter.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HourReporter.Services;

public class TogglApiService : ITogglApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<TogglApiService> _logger;
    private readonly string _apiToken;
    private const string BaseUrl = "https://api.track.toggl.com/api/v9";

    public TogglApiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TogglApiService> logger
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _apiToken =
            configuration.GetValue<string>("TogglApiToken")
            ?? throw new InvalidOperationException("TogglApiToken not configured");

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiToken}:api_token"));
        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "HourReporter/1.0");
    }

    public async Task<TimeEntry[]> GetTimeEntriesAsync(
        DateTime startDate,
        DateTime endDate,
        string? projectName = null,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            _logger.LogInformation(
                "Fetching time entries from {StartDate} to {EndDate}",
                startDate,
                endDate
            );

            var timeEntries = await GetTimeEntriesAsync(
                startDate,
                endDate,
                cancellationToken
            );

            var filteredEntries = FilterTimeEntries(timeEntries, projectName);

            _logger.LogInformation(
                "Found {Count} time entries matching criteria",
                filteredEntries.Count
            );
            return filteredEntries.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching time entries");
            throw;
        }
    }

    private async Task<List<TimeEntry>> GetTimeEntriesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken
    )
    {
        var startDateString = startDate.ToString("o");
        var endDateString = endDate.AddDays(1).AddSeconds(-1).ToString("o");

        var url =
            $"{BaseUrl}/me/time_entries?start_date={startDateString}Z&end_date={endDateString}Z&meta=true&include_sharing=true";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        var timeEntries =
            JsonSerializer.Deserialize<TimeEntry[]>(
                content,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower }
            ) ?? Array.Empty<TimeEntry>();

        return timeEntries.ToList();
    }

    private static List<TimeEntry> FilterTimeEntries(
        List<TimeEntry> timeEntries,
        string? projectName
    )
    {
        if (string.IsNullOrEmpty(projectName))
            return timeEntries;

        return timeEntries
            .Where(entry =>
            {
                if (!string.IsNullOrEmpty(projectName) && entry.ProjectName != null)
                    return entry.ProjectName.Equals(
                        projectName,
                        StringComparison.OrdinalIgnoreCase
                    );

                return false;
            })
            .ToList();
    }
}
