using System.Threading.Tasks;
using System.Collections.Generic;
using MailjetApiClient.Models;

namespace MailjetApiClient;

public interface IMailjetApiClient
{
    Task SendMail(MailjetMail mailjetMail);
    Task SendMail(string email, int templateId, object variables = null, List<User> UsersInCc = null, List<User> UsersInBcc = null);
    Task AddOrUpdateContact(MailjetContact mailjetContact);
    Task<int?> GetContactId(string contactEmail);
    Task DeleteContactFromContactList(string contactEmail, string contactListId);
}