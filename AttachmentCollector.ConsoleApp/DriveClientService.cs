using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using File = Google.Apis.Drive.v3.Data.File;

namespace AttachmentCollector.ConsoleApp;

public class DriveClientService(UserCredential credential, string userId)
{
    private readonly DriveService _driveService = new(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "AttachmentCollector"
    });

    public Task UploadFile(string fileName, Stream fileStream)
    {
        var fileMimeType = MimeTypeHelper.GetMimeType(fileName);
        var folderId = CreateFolder(fileName.Split('.').First());
        
        var file = new File()
        {
            Name = fileName,
            MimeType = fileMimeType,
            Parents = new string[] { folderId }
        };
        
        var fileResponse = _driveService.Files.Create(file, fileStream, fileMimeType) ?? throw new ApplicationException("File could not be uploaded.");
        
        return Task.CompletedTask;
    }
    
    private string CreateFolder(string folderName)
    {
        var folder = new File()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
        };
        
        var folderResponse = _driveService.Files.Create(folder).Execute() ?? throw new ApplicationException("Failed to create folder");
        
        return folderResponse.Id;
    }
}