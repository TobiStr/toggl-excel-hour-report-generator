# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 8 Azure Functions application that generates Excel reports from Toggl time tracking data. The application exposes a single HTTP endpoint that accepts date ranges and project filters, fetches data from the Toggl API, and returns a formatted Excel file using ClosedXML.

## Architecture

The solution follows a clean architecture pattern with:

- **HourReporter** - Main Azure Functions project with HTTP trigger function
- **HourReporter.Test** - XUnit test project with unit and integration tests
- **Services** - Service layer with interfaces for dependency injection:
  - `ITogglApiService` - Handles API calls to Toggl Track
  - `IReportService` - Orchestrates data processing and filtering
  - `IExcelReportService` - Generates Excel files using ClosedXML
- **Models** - Data transfer objects for API requests/responses and domain entities

## Common Commands

### Build and Test
```bash
# Build the solution
dotnet build

# Run all tests
dotnet test

# Run specific test project
dotnet test test/HourReporter.Test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Local Development
```bash
# Run the Azure Functions locally (requires Azure Functions Core Tools)
func start --cwd src/HourReporter

# Alternative: Use dotnet to run the function
dotnet run --project src/HourReporter
```

### Testing the API Locally
The function runs at `http://localhost:7071/api/report` and expects a POST request with JSON body containing `startDate`, `endDate`, and `projectName`.

## Configuration

- **Local Development**: Configure `TogglApiToken` in `src/HourReporter/local.settings.json`
- **Production**: Set `TogglApiToken` as an Azure Function App setting
- The API token is obtained from Toggl Track Profile Settings → API Token

## Dependencies

- **.NET 8** - Main framework (HourReporter project)
- **.NET 9** - Test project framework  
- **Azure Functions v4** - Serverless hosting platform
- **ClosedXML** - Excel generation library (commercially licensable)
- **XUnit + Moq** - Testing framework with mocking
- **Application Insights** - Telemetry and monitoring

## Key Files

- `src/HourReporter/Functions/HourReportFunction.cs` - Main HTTP trigger function
- `src/HourReporter/Program.cs` - Dependency injection configuration
- `API_USAGE.md` - Comprehensive API documentation with examples
- `README.md` - Basic project description