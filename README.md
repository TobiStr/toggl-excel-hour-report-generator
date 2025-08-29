# Toggl Excel Hour Report Generator API

## Overview

This Azure Function generates Excel reports from Toggl time tracking data using ClosedXML (commercially licensable). It accepts a date range and filter criteria, then fetches time entries from the Toggl API and generates a formatted Excel file.

## Configuration

### Required Settings

Add your Toggl API token to the application settings:

**local.settings.json** (for local development):

```json
{
  "Values": {
    "TogglApiToken": "your_toggl_api_token_here"
  }
}
```

**Azure Function App Settings** (for production):

- Key: `TogglApiToken`
- Value: Your Toggl API token

### Getting Your Toggl API Token

1. Log into your Toggl Track account
2. Go to Profile Settings → API Token
3. Copy the token and add it to your configuration

## API Endpoint

### Generate Hour Report

**Endpoint:** `POST /api/report`  
**Authorization:** Function key required  
**Content-Type:** `application/json`

#### Request Body

```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "projectName": "Project Name"
}
```

#### Request Examples

**Filter by Client:**

```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "projectName": "Acme Corp"
}
```

**Filter by Project:**

```json
{
  "startDate": "2024-01-01",
  "endDate": "2024-01-31",
  "projectName": "Website Redesign"
}
```

#### Response

**Success (200 OK):**

- Content-Type: `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`
- Content-Disposition: `attachment; filename="HourReport_ClientName_January_2024.xlsx"`
- Body: Excel file binary data

**Error Response:**

```json
{
  "error": "Error message describing what went wrong",
  "timestamp": "2024-01-15T10:30:00.000Z"
}
```

## Excel Report Format

The generated Excel file contains:

- **Worksheet Name:** "Report"
- **Filename Format:** `HourReport_{ProjectName}_{MonthName}_{Year}.xlsx`

### Columns

1. **Date** - Date of the time entry
2. **Project** - Project name
3. **Description** - Task description
4. **Duration (Hours)** - Time spent in decimal hours
5. **Tags** - Associated tags (comma-separated)
6. **Start Time** - Start time (HH:MM format)
7. **End Time** - End time (HH:MM format)

### Summary Section

At the bottom of the report:

- Report period
- Filter applied
- Total hours
- Total entries count

## Testing the API

### Using curl

```bash
curl -X POST "https://your-function-app.azurewebsites.net/api/report?code=your-function-key" \
  -H "Content-Type: application/json" \
  -d '{
    "startDate": "2024-01-01",
    "endDate": "2024-01-31",
    "projectName": "Test"
  }' \
  --output report.xlsx
```

### Using PowerShell

```powershell
$body = @{
    startDate = "2024-01-01"
    endDate = "2024-01-31"
    projectName = "Test"
} | ConvertTo-Json

Invoke-WebRequest -Uri "https://your-function-app.azurewebsites.net/api/report?code=your-function-key" `
  -Method POST `
  -Body $body `
  -ContentType "application/json" `
  -OutFile "report.xlsx"
```

## Error Handling

### Common Errors

- **400 Bad Request:** Invalid request format or missing required parameters
- **502 Bad Gateway:** Error accessing Toggl API (check API token)
- **500 Internal Server Error:** Unexpected server error

### Troubleshooting

1. **"Invalid request parameters"** - Ensure startDate ≤ endDate and either clientName or projectName is provided
2. **"Error accessing Toggl API"** - Verify your API token is correct and has proper permissions
3. **No time entries found** - Check if entries exist for the specified date range and filters

## Running Locally

1. Set up your `local.settings.json` with the Toggl API token
2. Run the function app:
   ```bash
   func start
   ```
3. Test the endpoint at: `http://localhost:7071/api/report`

## Deployment

1. Deploy to Azure Functions
2. Configure the `TogglApiToken` application setting
3. Test the deployed function with your function key
