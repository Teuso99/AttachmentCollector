namespace AttachmentCollector.ConsoleApp.Services;

public interface IGmailClientService
{
    List<AttachmentDTO> GetAttachments();
}