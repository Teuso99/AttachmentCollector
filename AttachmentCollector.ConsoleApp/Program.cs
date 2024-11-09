using AttachmentCollector.ConsoleApp;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;
using Microsoft.IdentityModel.Tokens;

if (args[0] is null)
{
    throw new ApplicationException("Enter the secret's file name");
}

var secretFile = Environment.CurrentDirectory + "\\" + args[0];

const string userId = "me";
string[] scopes = [DriveService.Scope.Drive, GmailService.Scope.GmailModify];

var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromFile(secretFile).Secrets, scopes, "user", CancellationToken.None);

if (credential.Token.IsStale)
{
    await credential.RefreshTokenAsync(CancellationToken.None);
}

var gmailClient = new GmailClientService(credential, userId);
var driveClient = new DriveClientService(credential);

var attachments = gmailClient.GetAttachments();

foreach (var attachment in attachments)
{
    await driveClient.UploadFile(attachment);
}