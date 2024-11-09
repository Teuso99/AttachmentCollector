using Google.Apis.Gmail.v1.Data;

namespace AttachmentCollector.ConsoleApp;

public static class MessageExtension
{
    /// <summary>
    /// Gets the subject of the specified message
    /// </summary>
    /// <param name="message">An instance of the Message class from the Gmail Client Package</param>
    /// <returns>The subject of the message; If it fails to retrieve the data then an empty string</returns>
    public static string GetMailSubject(this Message message)
    {
        return message.Payload?.Headers?.FirstOrDefault(h => h.Name == "Subject")?.Value ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the sender of the specified message
    /// </summary>
    /// <param name="message">An instance of the Message class from the Gmail Client Package</param>
    /// <returns>The sender of the message; If it fails to retrieve the data then an empty string</returns>
    public static string GetMailSender(this Message message)
    {
        return message.Payload?.Headers?.FirstOrDefault(h => h.Name == "From")?.Value ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the recipient of the specified message
    /// </summary>
    /// <param name="message">An instance of the Message class from the Gmail Client Package</param>
    /// <returns>The recipient of the message; If it fails to retrieve the data then an empty string</returns>
    public static string GetMailRecipient(this Message message)
    {
        return message.Payload?.Headers?.FirstOrDefault(h => h.Name == "To")?.Value ?? string.Empty;
    }
}