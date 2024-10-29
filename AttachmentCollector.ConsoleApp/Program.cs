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
string secretFile = secretFilePath + "\\" + secretFileName;

const string userId = "me";
const string labelName = "AttachmentCollector";
string[] scopes = [GmailService.Scope.GmailModify];

var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.FromFile(secretFile).Secrets, scopes, "user", CancellationToken.None);

if (credential.Token.IsStale)
{
    await credential.RefreshTokenAsync(CancellationToken.None);
}

GmailService service = new(new BaseClientService.Initializer()
{
    HttpClientInitializer = credential,
    ApplicationName = "AttachmentCollector"
});


var labelList = service.Users.Labels.List(userId).Execute() ?? throw new ApplicationException("Unexpected error when retrieving the label list");

var attachmentCollectorLabel = labelList.Labels.FirstOrDefault(l => l.Name == labelName);

var label = attachmentCollectorLabel ?? CreateLabel();


var listMessageRequest = service.Users.Messages.List(userId);
listMessageRequest.LabelIds = "INBOX";
listMessageRequest.IncludeSpamTrash = false;
listMessageRequest.Q = "has:attachment";

var listMessageResponse = listMessageRequest.Execute() ?? throw new ApplicationException("Unexpected error when listing messages from the user's inbox");

if (listMessageResponse.Messages is null || listMessageResponse.Messages.Count == 0)
{
    Console.WriteLine("No messages with attachments found");
    return;
}

List<string> attachmentsData = [];
List<string> addLabelsList = [label.Id];
ModifyMessageRequest modifyMessageRequest = new()
{ 
    AddLabelIds = addLabelsList
};
var messagesIds = listMessageResponse.Messages.Select(m => m.Id);


foreach (var messageId in messagesIds)
{
    var message = service.Users.Messages.Get(userId, messageId).Execute();

    var attachments = GetAttachments(message).ToList();

    if (attachments.Count == 0)
    {
        continue;
    }

    attachmentsData.AddRange(attachments.Select(a => a.Data));

    var modifyMessageResponse = service.Users.Messages.Modify(modifyMessageRequest, userId, messageId).Execute();

    if (modifyMessageResponse is null)
    {
        throw new ApplicationException("Unexpected error when adding label to the message " + message.Id);
    }
}

Console.WriteLine(attachmentsData.Count);
return;

Label CreateLabel()
{
    return service.Users.Labels.Create(new Label() { Name = "AttachmentCollector" }, userId).Execute() ?? throw new ApplicationException("Unexpected error when creating the app's label");
}

IEnumerable<MessagePartBody> GetAttachments(Message message)
{
    if (message.Payload is null)
    {
        throw new ApplicationException("No payload found in message " + message.Id);
    }

    var attachmentParts = message.Payload.Parts.Where(p => !string.IsNullOrWhiteSpace(p.Filename) && !string.IsNullOrWhiteSpace(p.Body?.Data)).ToList();

    if (attachmentParts.Count > 0)
    {
        return attachmentParts.Select(ap => ap.Body);
    }

    var attachmentIdParts = message.Payload.Parts.Where(p => !string.IsNullOrWhiteSpace(p.Body?.AttachmentId)).ToList();

    if (attachmentIdParts is null || attachmentIdParts.Count == 0)
    {
        throw new ApplicationException("No attachment found in message " + message.Id);
    }

    List<MessagePartBody> attachments = [];

    foreach (var attachmentIdPart in attachmentIdParts)
    {
        var attachmentId = attachmentIdPart.Body.AttachmentId;

        var attachment = service.Users.Messages.Attachments.Get(userId, message.Id, attachmentId).Execute();

        if (attachment is null || string.IsNullOrWhiteSpace(attachment.Data))
        {
            continue;
        }

        attachments.Add(attachment);
    }

    return attachments;
}