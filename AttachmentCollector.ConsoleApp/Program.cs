using AttachmentCollector.ConsoleApp;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Microsoft.IdentityModel.Tokens;

if (args[0] is null)
{
    throw new ApplicationException("Enter the secret's file name");
}

var secretFile = Environment.CurrentDirectory + "\\" + args[0];

const string userId = "me";
string[] scopes = [GmailService.Scope.GmailModify];

var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromFile(secretFile).Secrets, scopes, "user", CancellationToken.None);

if (credential.Token.IsStale)
{
    await credential.RefreshTokenAsync(CancellationToken.None);
}

var gmailClient = new GmailClientService(credential, userId);

var attachmentsMetadata = gmailClient.GetAttachments();

foreach (var attachmentMetadata in attachmentsMetadata)
{
    await File.WriteAllBytesAsync(Environment.CurrentDirectory + "\\" + attachmentMetadata.Key, Base64UrlEncoder.DecodeBytes(attachmentMetadata.Value));
}