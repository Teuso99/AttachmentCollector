namespace AttachmentCollector.ConsoleApp;

public static class MimeTypeHelper
{
    private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { ".txt", "text/plain" },
        { ".html", "text/html" },
        { ".htm", "text/html" },
        { ".jpg", "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png", "image/png" },
        { ".gif", "image/gif" },
        { ".pdf", "application/pdf" },
        { ".doc", "application/msword" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".xls", "application/vnd.ms-excel" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { ".json", "application/json" },
        { ".xml", "application/xml" },
        { ".zip", "application/zip" },
        { ".rar", "application/vnd.rar" },
        { ".mp3", "audio/mpeg" },
        { ".mp4", "video/mp4" },
        { ".avi", "video/x-msvideo" }
    };

    public static string GetMimeType(string fileName)
    {
        var extension = fileName.Split('.').Last().ToLower();
        return MimeTypes.GetValueOrDefault(extension, "application/octet-stream"); // Default MIME type if unknown
    }
}