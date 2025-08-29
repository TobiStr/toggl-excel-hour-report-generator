using HourReporter.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register HTTP clients
builder.Services.AddHttpClient<ITogglApiService, TogglApiService>();

// Register services
builder.Services.AddScoped<ITogglApiService, TogglApiService>();
builder.Services.AddScoped<IExcelReportService, ExcelReportService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Build().Run();
