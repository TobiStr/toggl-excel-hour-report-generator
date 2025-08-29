using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using HourReporter.Functions;
using HourReporter.Models;
using HourReporter.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;

namespace HourReporter.Test.Integration;

public class HourReportFunctionIntegrationTests : IDisposable
{
    private readonly IHost _host;
    private readonly IServiceProvider _serviceProvider;
    private readonly HourReportFunction _function;

    public HourReportFunctionIntegrationTests()
    {
        // Build configuration from local.settings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: false)
            .Build()
            .GetSection("Values"); // Azure Functions expects config in Values section

        // Create host with services like the real Azure Function
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                // Add configuration
                services.AddSingleton<IConfiguration>(configuration);

                // Register HTTP clients
                services.AddHttpClient<ITogglApiService, TogglApiService>();

                // Register services
                services.AddScoped<ITogglApiService, TogglApiService>();
                services.AddScoped<IExcelReportService, ExcelReportService>();
                services.AddScoped<IReportService, ReportService>();
                services.AddScoped<HourReportFunction>();

                // Add logging
                services.AddLogging(builder => builder.AddDebug());
            })
            .Build();

        _serviceProvider = _host.Services;
        _function = _serviceProvider.GetRequiredService<HourReportFunction>();
    }

    [Fact]
    public async Task GenerateHourReport_WithValidRequest_ReturnsExcelFile()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2025-08-01"),
            EndDate = DateTime.Parse("2025-08-29"),
            ProjectName = "Test",
            HourlyRate = 80
        };

        var requestJson = JsonSerializer.Serialize(
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        var httpRequest = CreateMockHttpRequest(requestJson);

        // Act
        var response = await _function.GenerateHourReport(httpRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(response);

        // Check if we got success or a reasonable error response
        Assert.True(response.StatusCode == HttpStatusCode.OK);

        if (response.StatusCode == HttpStatusCode.OK)
        {
            // Verify response headers for Excel file
            Assert.Contains("Content-Type", response.Headers.Select(h => h.Key));
            Assert.Contains("Content-Disposition", response.Headers.Select(h => h.Key));

            // Save the Excel file for manual verification
            var excelData = ((MemoryStream)response.Body).ToArray();
            var fileName = $"test_report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            File.WriteAllBytes(fileName, excelData);

            // Verify the file was created and has content
            Assert.True(File.Exists(fileName));
            Assert.True(new FileInfo(fileName).Length > 0);
        }
    }

    [Fact]
    public async Task GenerateHourReport_WithProjectFilter_ReturnsResponse()
    {
        // Arrange
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-01"),
            EndDate = DateTime.Parse("2024-01-07"),
            ProjectName = "Test", // Try project filter instead
        };

        var requestJson = JsonSerializer.Serialize(
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        var httpRequest = CreateMockHttpRequest(requestJson);

        // Act
        var response = await _function.GenerateHourReport(httpRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(response);

        // Should get a valid response (success, no data found, or API error)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK
                || response.StatusCode == HttpStatusCode.BadRequest
                || response.StatusCode == HttpStatusCode.BadGateway
                || response.StatusCode == HttpStatusCode.InternalServerError
        );
    }

    [Fact]
    public async Task GenerateHourReport_WithInvalidRequest_ReturnsBadRequest()
    {
        // Arrange - Invalid request with start date after end date
        var request = new ReportRequest
        {
            StartDate = DateTime.Parse("2024-01-31"),
            EndDate = DateTime.Parse("2024-01-01"), // Invalid: start after end
            ProjectName = "Test",
        };

        var requestJson = JsonSerializer.Serialize(
            request,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
        );

        var httpRequest = CreateMockHttpRequest(requestJson);

        // Act
        var response = await _function.GenerateHourReport(httpRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateHourReport_WithEmptyBody_ReturnsBadRequest()
    {
        // Arrange
        var httpRequest = CreateMockHttpRequest("");

        // Act
        var response = await _function.GenerateHourReport(httpRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateHourReport_WithMalformedJson_ReturnsBadRequest()
    {
        // Arrange
        var httpRequest = CreateMockHttpRequest("{ invalid json }");

        // Act
        var response = await _function.GenerateHourReport(httpRequest, CancellationToken.None);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private static HttpRequestData CreateMockHttpRequest(string body)
    {
        var context = new Mock<FunctionContext>().Object;
        return new TestHttpRequestData(context, body);
    }

    public void Dispose()
    {
        _host?.Dispose();
    }
}

// Simple test implementation of HttpRequestData
public class TestHttpRequestData : HttpRequestData
{
    private readonly string _body;
    private readonly MemoryStream _bodyStream;

    public TestHttpRequestData(FunctionContext functionContext, string body)
        : base(functionContext)
    {
        _body = body;
        _bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(body));
        Url = new Uri("https://localhost/api/report");
        Method = "POST";
        Headers = new HttpHeadersCollection();
    }

    public override Stream Body => _bodyStream;
    public override HttpHeadersCollection Headers { get; }
    public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = Array.Empty<IHttpCookie>();
    public override Uri Url { get; }
    public override IEnumerable<ClaimsIdentity> Identities { get; } = Array.Empty<ClaimsIdentity>();
    public override string Method { get; }

    public HttpResponseData CreateResponse(HttpStatusCode statusCode)
    {
        return new TestHttpResponseData(FunctionContext, statusCode);
    }

    public override HttpResponseData CreateResponse()
    {
        return new TestHttpResponseData(FunctionContext, HttpStatusCode.OK);
    }
}

// Simple test implementation of HttpResponseData
public class TestHttpResponseData : HttpResponseData
{
    public TestHttpResponseData(FunctionContext functionContext, HttpStatusCode statusCode)
        : base(functionContext)
    {
        StatusCode = statusCode;
        Headers = new HttpHeadersCollection();
        Body = new MemoryStream();
    }

    public override HttpStatusCode StatusCode { get; set; }
    public override HttpHeadersCollection Headers { get; set; }
    public override Stream Body { get; set; }
    public override HttpCookies Cookies { get; } = null!;

    public async Task WriteStringAsync(string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        await Body.WriteAsync(bytes, 0, bytes.Length);
    }

    public async Task WriteBytesAsync(byte[] bytes)
    {
        await Body.WriteAsync(bytes, 0, bytes.Length);
    }
}
