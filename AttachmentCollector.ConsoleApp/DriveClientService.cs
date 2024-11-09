using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace AttachmentCollector.ConsoleApp;

public class DriveClientService(UserCredential credential)
{
    private readonly DriveService _driveService = new(new BaseClientService.Initializer()
    {
        HttpClientInitializer = credential,
        ApplicationName = "AttachmentCollector"
    });

    public async Task UploadFile(AttachmentDTO attachment)
    {
        var folderListRequest = _driveService.Files.List();
        folderListRequest.Q = "name = 'AttachmentCollector'";
        
        var folderList = await folderListRequest.ExecuteAsync();
        
        var baseFolderId = folderList is null || folderList.Files.Count == 0 ? CreateFolder("AttachmentCollector") : folderList.Files.First().Id;

        var folderId = CreateFolder(attachment.FolderName, baseFolderId);
        
        var file = new File()
        {
            Name = attachment.FileName,
            MimeType = attachment.MimeType,
            Parents = [folderId]
        };
        
        var fileResponse = await _driveService.Files.Create(file, attachment.FileStream, attachment.MimeType).UploadAsync() ?? throw new ApplicationException("File could not be uploaded.");

        if (fileResponse.Status != UploadStatus.Completed)
        {
            throw new ApplicationException("File could not be uploaded.");
        }
    }
    
    private string CreateFolder(string folderName, string? parentId = null)
    {
        var folder = new File()
        {
            Name = folderName,
            MimeType = "application/vnd.google-apps.folder",
        };

        if (parentId != null)
        {
            folder.Parents = new string[] { parentId };
        }
        
        var folderResponse = _driveService.Files.Create(folder).Execute() ?? throw new ApplicationException("Failed to create folder");
        
        return folderResponse.Id;
    }
}