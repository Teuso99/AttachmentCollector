using Google.Apis.Gmail.v1.Data;
using Microsoft.IdentityModel.Tokens;

namespace AttachmentCollector.ConsoleApp;

public class AttachmentDTO
{
    public string FolderName { get; private set; }
    public string FileName { get; private set; }
    public Stream FileStream { get; private set; }
    public string MimeType { get; private set; }

    public AttachmentDTO(Message message, MessagePart messagePart, MessagePartBody bodyPart)
    {
        var decodedFileData =  Base64UrlEncoder.DecodeBytes(bodyPart.Data);
        var fileStream = new MemoryStream(decodedFileData);
        
        FolderName = CreateFolderName(message);
        FileStream = fileStream;
        FileName = messagePart.Filename;
        MimeType = messagePart.MimeType;
    }

    private static string CreateFolderName(Message message)
    {
        var mailSubject = message.GetMailSubject();
        var mailSender = message.GetMailSender().RemoveEmailAddress();

        if (mailSender.Contains("reply", StringComparison.CurrentCultureIgnoreCase) || string.IsNullOrWhiteSpace(mailSender))
        {
            return mailSubject;
        }
        
        return mailSender; 
    }
}