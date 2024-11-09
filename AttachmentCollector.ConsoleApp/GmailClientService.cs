using System.Net.Mail;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.IdentityModel.Tokens;

namespace AttachmentCollector.ConsoleApp;

public class GmailClientService(UserCredential credential, string userId)
{
    private const string LabelName = "AttachmentCollector";
    
    private readonly GmailService _service = new(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "AttachmentCollector"
    });

    public List<AttachmentDTO> GetAttachments()
    {
        var label = GetLabel() ?? CreateLabel();

        var listMessageRequest = _service.Users.Messages.List(userId);
        listMessageRequest.LabelIds = "INBOX";
        listMessageRequest.IncludeSpamTrash = false;
        listMessageRequest.Q = "has:attachment";

        var listMessageResponse = listMessageRequest.Execute() ?? throw new ApplicationException("Unexpected error when listing messages from the user's inbox");

        if (listMessageResponse.Messages is null || listMessageResponse.Messages.Count == 0)
        {
            throw new ApplicationException("No messages with attachments found");
        }

        var attachmentList = new List<AttachmentDTO>();
        List<string> addLabelsList = [label.Id];
        ModifyMessageRequest modifyMessageRequest = new()
        { 
            AddLabelIds = addLabelsList
        };

        var messagesIds = listMessageResponse.Messages.Select(m => m.Id);

        foreach (var messageId in messagesIds)
        {
            var message = _service.Users.Messages.Get(userId, messageId).Execute();

            var attachments = GetAttachmentsFromMessage(message);

            if (attachments.Count == 0)
            {
                continue;
            }
    
            var modifyMessageResponse = _service.Users.Messages.Modify(modifyMessageRequest, userId, messageId).Execute();

            if (modifyMessageResponse is null)
            {
                throw new ApplicationException("Unexpected error when adding label to the message " + message.Id);
            }
    
            attachmentList.AddRange(attachments);
        }

        return attachmentList;
    }

    private Label? GetLabel()
    {
        var labelList = _service.Users.Labels.List(userId).Execute() ?? throw new ApplicationException("Unexpected error when retrieving the label list");

        var attachmentCollectorLabel = labelList.Labels.FirstOrDefault(l => l.Name == LabelName);

        return attachmentCollectorLabel;
    }
    
    private Label CreateLabel()
    {
        return _service.Users.Labels.Create(new Label() { Name = "AttachmentCollector" }, userId).Execute() ?? throw new ApplicationException("Unexpected error when creating the app's label");
    }

    private List<AttachmentDTO> GetAttachmentsFromMessage(Message message)
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

        var attachmentList = new List<AttachmentDTO>();

        foreach (var attachmentPart in attachmentParts)
        {
            var attachmentResponse = _service.Users.Messages.Attachments.Get(userId, message.Id, attachmentPart.Body.AttachmentId).Execute();

            if (attachmentResponse is null)
            {
                throw new ApplicationException("Unexpected error when collecting attachments from the message " + message.Id);
            }
        
            var attachment = new AttachmentDTO(message, attachmentPart, attachmentResponse);
            attachmentList.Add(attachment);
        }
    
        return attachmentList;
    }
}