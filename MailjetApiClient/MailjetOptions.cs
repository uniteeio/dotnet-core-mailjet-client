namespace MailjetApiClient
{
    public class MailjetOptions
    {                
        public string ApiKeyPublic { get; set; }
        public string ApiKeyPrivate { get; set; }
        public string SenderEmail { get; set; }   
        public string SenderName { get; set; }   
        public string TestingRedirectionMail { get; set; }
        public bool? IsSendingMailAllowed { get; set; }
    }
}