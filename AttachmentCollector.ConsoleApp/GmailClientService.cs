using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.IdentityModel.Tokens;

namespace AttachmentCollector.ConsoleApp;

public class GmailClientService(UserCredential credential, string userId)
{
    const string LabelName = "AttachmentCollector";
    
    private readonly GmailService _service = new(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "AttachmentCollector"
    });

    public Dictionary<string, string> GetAttachments()
    {
        var labelList = _service.Users.Labels.List(userId).Execute() ?? throw new ApplicationException("Unexpected error when retrieving the label list");

        var attachmentCollectorLabel = labelList.Labels.FirstOrDefault(l => l.Name == LabelName);

        var label = attachmentCollectorLabel ?? CreateLabel();

        var listMessageRequest = _service.Users.Messages.List(userId);
        listMessageRequest.LabelIds = "INBOX";
        listMessageRequest.IncludeSpamTrash = false;
        listMessageRequest.Q = "has:attachment";

        var listMessageResponse = listMessageRequest.Execute() ?? throw new ApplicationException("Unexpected error when listing messages from the user's inbox");

        if (listMessageResponse.Messages is null || listMessageResponse.Messages.Count == 0)
        {
            throw new ApplicationException("No messages with attachments found");
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
            var message = _service.Users.Messages.Get(userId, messageId).Execute();

            var attachments = GetAttachments(message);

            if (attachments.Count == 0)
            {
                continue;
            }
    
            var modifyMessageResponse = _service.Users.Messages.Modify(modifyMessageRequest, userId, messageId).Execute();

            if (modifyMessageResponse is null)
            {
                throw new ApplicationException("Unexpected error when adding label to the message " + message.Id);
            }
    
            attachmentsMetadata = attachmentsMetadata.Concat(attachments).ToDictionary();
        }

        return attachmentsMetadata;
    }
    
    private Label CreateLabel()
    {
        return _service.Users.Labels.Create(new Label() { Name = "AttachmentCollector" }, userId).Execute() ?? throw new ApplicationException("Unexpected error when creating the app's label");
    }

    private Dictionary<string, string> GetAttachments(Message message)
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
            var attachmentResponse = _service.Users.Messages.Attachments.Get(userId, message.Id, attachmentPart.Body.AttachmentId).Execute();

            if (attachmentResponse is null)
            {
                throw new ApplicationException("Unexpected error when collecting attachments from the message " + message.Id);
            }
        
            attachmentsDictionary.Add(attachmentPart.Filename, attachmentResponse.Data);
        }
    
        return attachmentsDictionary;
    }
}