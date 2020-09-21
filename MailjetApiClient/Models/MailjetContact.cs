namespace MailjetApiClient.Models
{
    public class MailjetContact
    {
        public bool IsExcluded { get; set; }
        public string ContactEmail { get; set; }
        public string ContactName { get; set; }
        public string ContactListId { get; set; } = "";
    }
}