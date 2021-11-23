namespace MailjetApiClient.Models;

public class MailAttachmentFile
{
    public string ContentType { get; set; }
    public string Filename { get; set; }
    public string Base64Content { get; set; }
}