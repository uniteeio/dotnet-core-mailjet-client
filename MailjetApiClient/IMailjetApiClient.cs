using System.Collections.Generic;
using System.Threading.Tasks;
using MailjetApiClient.Models;
using Newtonsoft.Json.Linq;

namespace MailjetApiClient
{
    public interface IMailjetApiClient
    {
        Task<bool> SendMail(IEnumerable<User> users, int templateId, JObject variables = null, MailAttachmentFile attachmentFile = null, List<User> usersInCc = null);
    }
}