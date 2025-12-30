namespace AttachmentCollector.ConsoleApp.Services;

public interface IDriveClientService
{
    Task UploadFile(AttachmentDTO attachment);
}