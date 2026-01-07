# AttachmentCollector

A .NET Console Application that automates the process of saving attachments from Gmail to Google Drive.

## Overview

This tool scans your Gmail inbox for messages with attachments, uploads the files to specific folders in Google Drive based on the sender or subject, and marks the emails with the `AttachmentCollector` label. Helping to identify processed messages in the inbox and ensuring that there is no duplicates. It is designed to run either as a local console tool or as an AWS Lambda function.

## Prerequisites

- .NET 8.0 SDK or later
- A Google Cloud Project with the following APIs enabled:
  - **Gmail API**
  - **Google Drive API**
- OAuth 2.0 Client ID and Client Secret from the Google Cloud Console.

## Configuration

The application requires Google API credentials to run. These are configured via environment variables.

1. Navigate to `AttachmentCollector.ConsoleApp/Properties`.
2. Create a copy of `launchSettings.Development.json` and rename it to `launchSettings.json`.
3. Update the `environmentVariables` section with your actual credentials:

```json
{
  "profiles": {
    "AttachmentCollector.ConsoleApp": {
      "commandName": "Project",
      "environmentVariables": {
        "GOOGLE_CLIENT_ID": "your-client-id.apps.googleusercontent.com",
        "GOOGLE_CLIENT_SECRET": "your-client-secret",
        "GOOGLE_REFRESH_TOKEN": "your-refresh-token"
      }
    }
  }
}
```

> **Note**: The `launchSettings.json` file is ignored by git to keep your secrets safe.

## How to Run

1. Open the solution in your preferred IDE (Visual Studio, Rider, VS Code).
2. Run the application using the `AttachmentCollector.ConsoleApp` as the startup project.

Alternatively, via CLI:

```bash
cd AttachmentCollector.ConsoleApp
dotnet run --launch-profile AttachmentCollector.ConsoleApp
```

## AWS Lambda Deployment

This application includes a `Function.cs` entry point for AWS Lambda.

For detailed instructions on how to package, deploy, and configure the function (including environment variables), please see [AWS_SETUP.md](AWS_SETUP.md).
