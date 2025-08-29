using System.Net;
using System.Text.Json;
using HourReporter.Models;
using HourReporter.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace HourReporter.Functions;

public class HourReportFunction
{
    private readonly IReportService _reportService;
    private readonly ILogger<HourReportFunction> _logger;

    public HourReportFunction(IReportService reportService, ILogger<HourReportFunction> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [Function("GenerateHourReport")]
    public async Task<HttpResponseData> GenerateHourReport(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "report")] HttpRequestData req,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received request to generate hour report");

        try
        {
            // Parse request body
            var requestBody = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Request body is empty");
            }

            var reportRequest = JsonSerializer.Deserialize<ReportRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            if (reportRequest == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request format");
            }

            // Validate request
            if (!reportRequest.IsValid())
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest,
                    "Invalid request parameters. StartDate must be <= EndDate, and ProjectName must be provided");
            }

            _logger.LogInformation("Generating report for period {StartDate} to {EndDate}, Project: {ProjectName}",
                reportRequest.StartDate, reportRequest.EndDate, reportRequest.ProjectName);

            // Generate report
            var (excelData, fileName) = await _reportService.GenerateHourReportAsync(reportRequest, cancellationToken);

            // Create response with Excel file
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            response.Headers.Add("Content-Disposition", $"attachment; filename=\"{fileName}\"");

            await response.WriteBytesAsync(excelData);

            _logger.LogInformation("Report generated successfully: {FileName}", fileName);
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request parameters");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error communicating with Toggl API");
            return await CreateErrorResponse(req, HttpStatusCode.BadGateway, "Error accessing Toggl API. Please check your API token and try again.");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing request JSON");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format in request body");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error generating report");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An unexpected error occurred while generating the report");
        }
    }

    private static async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode, string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json");

        var errorResponse = new
        {
            error = message,
            timestamp = DateTime.UtcNow
        };

        await response.WriteStringAsync(JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }));

        return response;
    }
}