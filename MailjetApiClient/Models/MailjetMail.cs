using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace MailjetApiClient.Models
{
    public class MailjetMail
    {
        public List<User> Users { get; set; }
        public int TemplateId { get; set; }
        public Dictionary<string, string> Variables { get; set; } 
        public List<MailAttachmentFile> AttachmentFiles { get; set; } = new List<MailAttachmentFile>();
        public List<User> UsersInCc { get; set; }
        public List<User> UsersInBcc { get; set; }
    }
}