# Deployment Guide

This document describes how to set up automated deployment of the Azure Function using GitHub Actions.

## GitHub Actions Workflow

The repository includes a GitHub Actions workflow (`.github/workflows/deploy-azure-function.yml`) that:

1. **Build and Test** - Runs on all pushes and pull requests
   - Restores dependencies
   - Builds the solution
   - Runs all tests

2. **Deploy** - Runs only on pushes to `main` branch
   - Builds and publishes the Azure Function
   - Deploys to Azure using publish profile

**Note**: You have to add your toggl API Key to the configuration of your function app. (see below)

## Required GitHub Secrets

Configure the following secrets in your GitHub repository settings (`Settings > Secrets and variables > Actions`):

### Azure Function App Secrets

- **`AZURE_FUNCTIONAPP_NAME`**
  - Your Azure Function App name
  - Example: `my-toggl-report-function`

- **`AZURE_FUNCTIONAPP_PUBLISH_PROFILE`**
  - Download from Azure Portal: Function App > Overview > Get publish profile
  - Paste the entire XML content as the secret value

### Azure CLI Authentication

- **`AZURE_CREDENTIALS`**
  - Service Principal credentials in JSON format
  - Create using Azure CLI:
  ```bash
  az ad sp create-for-rbac --name "GitHub-Actions-SP" --role contributor --scopes /subscriptions/{subscription-id}/resourceGroups/{resource-group} --sdk-auth
  ```
  - Use the JSON output as the secret value

### Application Configuration

- **`TOGGL_API_TOKEN`**
  - Your Toggl Track API token
  - Get from: Toggl Track > Profile Settings > API Token

## Setting Up Azure Resources

Before deploying, ensure you have:

1. **Azure Function App** - Create a Function App with:
   - Runtime: .NET 8
   - Plan: Consumption
   - Storage Account configured

2. **Resource Group** - The Function App should be in a dedicated resource group

3. **Service Principal** - With Contributor access to the resource group

## Manual Deployment Alternative

If you prefer manual deployment:

```bash
# Build and publish locally
dotnet publish src/HourReporter --configuration Release --output ./publish

# Deploy using Azure CLI
az functionapp deployment source config-zip \
  --resource-group {resource-group} \
  --name {function-app-name} \
  --src ./publish.zip

# Configure app settings
az functionapp config appsettings set \
  --name {function-app-name} \
  --resource-group {resource-group} \
  --settings "TogglApiToken={your-token}"
```

## Environment Protection

The workflow uses GitHub Environments for additional security:
- `production` environment protects the main branch deployment
- Consider adding required reviewers for production deployments
- Configure environment secrets if different from repository secrets

## Monitoring Deployment

Monitor deployment status:
1. GitHub Actions tab shows workflow execution
2. Azure Portal > Function App > Deployment Center shows deployment history
3. Application Insights (if configured) provides runtime monitoring

## Troubleshooting

Common issues:
- **Build failures**: Check .NET version compatibility and dependencies
- **Deployment failures**: Verify publish profile and Function App settings
- **Runtime errors**: Check Application Settings and connection strings in Azure Portal
- **Permission errors**: Ensure Service Principal has sufficient Azure permissions