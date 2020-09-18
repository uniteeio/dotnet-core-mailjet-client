using System.Threading.Tasks;
using MailjetApiClient.Models;

namespace MailjetApiClient
{
    public interface IMailjetApiClient
    {
        Task<bool> SendMail(IEnumerable<User> users, int templateId, JObject variables = null, MailAttachmentFile attachmentFile = null, List<User> usersInCc = null);
        Task<int?> AddContact(bool isExcluded, string contactName, string contactEmail, string contactListID = "");
        Task<int?> GetContactID(string contactEmail);
        Task<bool> DeleteContactFromContactList(string contactEmail, string contactListID);
        Task TestHttp();
        Task<bool> SendMail(MailjetMail mailjetMail);
    }
}