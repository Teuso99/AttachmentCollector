using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.IdentityModel.Tokens;

if (args[0] is null)
{
    throw new ApplicationException("Enter the secret's file name");
}

string secretFile = Environment.CurrentDirectory + "\\" + args[0];

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

Dictionary<string, string> attachmentsMetadata = new();
List<string> addLabelsList = [label.Id];
ModifyMessageRequest modifyMessageRequest = new()
{ 
    AddLabelIds = addLabelsList
};

var messagesIds = listMessageResponse.Messages.Select(m => m.Id);

foreach (var messageId in messagesIds)
{
    var message = service.Users.Messages.Get(userId, messageId).Execute();

    var attachments = GetAttachments(message);

    if (attachments.Count == 0)
    {
        continue;
    }
    
    var modifyMessageResponse = service.Users.Messages.Modify(modifyMessageRequest, userId, messageId).Execute();

    if (modifyMessageResponse is null)
    {
        throw new ApplicationException("Unexpected error when adding label to the message " + message.Id);
    }
    
    attachmentsMetadata = attachmentsMetadata.Concat(attachments).ToDictionary();
}

foreach (var attachmentMetadata in attachmentsMetadata)
{
    await File.WriteAllBytesAsync(Environment.CurrentDirectory + "\\" + attachmentMetadata.Key, Base64UrlEncoder.DecodeBytes(attachmentMetadata.Value));
}

return;

Label CreateLabel()
{
    return service.Users.Labels.Create(new Label() { Name = "AttachmentCollector" }, userId).Execute() ?? throw new ApplicationException("Unexpected error when creating the app's label");
}

Dictionary<string, string> GetAttachments(Message message)
{
    if (message.Payload is null)
    {
        throw new ApplicationException("No payload found in message " + message.Id);
    }

    var attachmentParts = message.Payload.Parts.Where(p => !string.IsNullOrWhiteSpace(p.Filename) && !string.IsNullOrWhiteSpace(p.Body?.AttachmentId)).ToList();

    if (attachmentParts.Count == 0)
    {
        throw new ApplicationException("Unexpected error when collecting attachments from the message " + message.Id);
    }

    var attachmentsDictionary = new Dictionary<string, string>();

    foreach (var attachmentPart in attachmentParts)
    {
        var attachmentResponse = service.Users.Messages.Attachments.Get(userId, message.Id, attachmentPart.Body.AttachmentId).Execute();

        if (attachmentResponse is null)
        {
            throw new ApplicationException("Unexpected error when collecting attachments from the message " + message.Id);
        }
        
        attachmentsDictionary.Add(attachmentPart.Filename, attachmentResponse.Data);
    }
    
    return attachmentsDictionary;
}