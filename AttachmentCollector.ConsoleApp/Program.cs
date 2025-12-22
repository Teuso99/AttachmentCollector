using AttachmentCollector.ConsoleApp;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Gmail.v1;

const string userId = "me";
string[] scopes = [DriveService.Scope.Drive, GmailService.Scope.GmailModify];

var credential = new UserCredential(
    new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
    {
        ClientSecrets = new ClientSecrets
        {
            ClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID"),
            ClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET")
        },
        Scopes = scopes
    }),
    userId,
    new TokenResponse { RefreshToken = Environment.GetEnvironmentVariable("GOOGLE_REFRESH_TOKEN") }
);

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