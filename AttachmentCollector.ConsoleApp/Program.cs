using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;

if (args[0] is null)
{
    throw new ApplicationException("Enter the secret's file name");
}

string secretFileName = args[0];
string secretFilePath = Environment.CurrentDirectory;

const string USER_ID = "me";

string[] scopes = { GmailService.Scope.GmailModify };


string secretFile = secretFilePath + "\\" + secretFileName;

UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromFile(secretFile).Secrets, scopes, "user", CancellationToken.None);

if (credential.Token.IsStale)
{
    await credential.RefreshTokenAsync(CancellationToken.None);
}

GmailService service = new GmailService(new BaseClientService.Initializer()
{
    HttpClientInitializer = credential,
    ApplicationName = "AttachmentCollector"
});

var request = service.Users.Messages.List(USER_ID);
request.LabelIds = "INBOX";
request.IncludeSpamTrash = false;
request.Q = "has:attachment";

var response = request.Execute();

if (response is null)
{
    throw new ApplicationException("Enter the secret's file name");
}

if (response.Messages is null || response.Messages.Count == 0)
{
    Console.WriteLine("No messages with attachments found");
    return;
}

List<string> attachmentsData = new List<string>();
var messagesIds = response.Messages.Select(m => m.Id);

foreach (var messageId in messagesIds)
{
    var message = service.Users.Messages.Get(USER_ID, messageId).Execute();

    var attachment = GetAttachment(message);

    if (string.IsNullOrWhiteSpace(attachment?.Data))
    {
        continue;
    }

    attachmentsData.Add(attachment.Data);
}

Console.WriteLine(attachmentsData.Count);

MessagePartBody GetAttachment(Message message)
{
    if (message.Payload is null)
    {
        throw new ApplicationException("fudeu");
    }

    var messageWithAttachment = message.Payload.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Filename) && !string.IsNullOrWhiteSpace(p.Body?.Data));

    if (messageWithAttachment != null)
    {
        return messageWithAttachment.Body;
    }

    var attachmentId = message.Payload.Parts.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Body?.AttachmentId))?.Body.AttachmentId;

    var attachment = service.Users.Messages.Attachments.Get(USER_ID, message.Id, attachmentId).Execute();

    return attachment ?? throw new ApplicationException("No attachment found in message " + message.Id);
}