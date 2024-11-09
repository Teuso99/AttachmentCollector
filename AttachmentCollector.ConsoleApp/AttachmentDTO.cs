using Google.Apis.Gmail.v1.Data;
using Microsoft.IdentityModel.Tokens;

namespace AttachmentCollector.ConsoleApp;

public class AttachmentDTO
{
    public string FolderName { get; set; }
    public string FileName { get; set; }
    public Stream FileStream { get; set; }
    public string MimeType { get; set; }

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
        var mailSender = message.GetMailSender();
        
        var folderName = DateTime.Now.ToString("yyyyMMdd");

        if (mailSender.Contains("reply", StringComparison.CurrentCultureIgnoreCase))
        {
            return folderName + "_" + mailSubject;
        }
        
        return folderName + "_" + mailSender; 
    }
}