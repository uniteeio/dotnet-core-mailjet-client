namespace MailjetApiClient;

public class MailjetService : MailjetApiClient
{
    public MailjetService(MailjetOptions options) : base(options.ApiKeyPublic, options.ApiKeyPrivate, options.SenderEmail, options.SenderName, options.TestingRedirectionMail, options.IsSendingMailAllowed) {}
}