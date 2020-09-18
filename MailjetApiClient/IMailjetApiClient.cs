using System.Threading.Tasks;
using MailjetApiClient.Models;

namespace MailjetApiClient
{
    public interface IMailjetApiClient
    {
        Task<bool> SendMail(MailjetMail mailjetMail);
        Task<int?> AddContact(MailjetContact mailjetContact);
        Task<int?> GetContactId(string contactEmail);
        Task<bool> DeleteContactFromContactList(string contactEmail, string contactListId);
    }
}