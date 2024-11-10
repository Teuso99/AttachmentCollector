using System.Text.RegularExpressions;

namespace AttachmentCollector.ConsoleApp;

public static partial class StringExtension
{
    public static string RemoveEmailAddress(this string sender)
    {
        sender = RemoveEmailAddressRegex().Replace(sender, "");
        sender = sender.Replace("<", "").Replace(">", "");
        
        return sender;
    }

    [GeneratedRegex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b")]
    private static partial Regex RemoveEmailAddressRegex();
}