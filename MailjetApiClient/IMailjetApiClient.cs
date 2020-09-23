using System.Threading.Tasks;
using MailjetApiClient.Models;

namespace MailjetApiClient
{
    public interface IMailjetApiClient
    {
        Task SendMail(MailjetMail mailjetMail);
        Task AddOrUpdateContact(MailjetContact mailjetContact);
        Task<int?> GetContactId(string contactEmail);
        Task DeleteContactFromContactList(string contactEmail, string contactListId);
    }
}