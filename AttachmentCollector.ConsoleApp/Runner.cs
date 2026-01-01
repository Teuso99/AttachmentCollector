using AttachmentCollector.ConsoleApp.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;

namespace AttachmentCollector.ConsoleApp;

public class Runner(IGmailClientService gmailClient, IDriveClientService driveClient, UserCredential credential, ILogger<Runner> logger)
{
    private readonly IGmailClientService _gmailClient = gmailClient;
    private readonly IDriveClientService _driveClient = driveClient;
    private readonly UserCredential _credential = credential;
    private readonly ILogger<Runner> _logger = logger;
    
    public async Task RunAsync()
    {
        if (_credential.Token.IsStale)
        {
            _logger.Log(LogLevel.Information, "Refreshing token");
            await _credential.RefreshTokenAsync(CancellationToken.None);
        }

        try
        {
            var attachments = _gmailClient.GetAttachments();

            foreach (var attachment in attachments)
            {
                await _driveClient.UploadFile(attachment);
            }
        }
        catch (ApplicationException ex)
        {
            _logger.Log(LogLevel.Warning, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.Log(LogLevel.Error, $"Unexpected error occurred. {ex.Message}");
            throw;
        }
    }
}