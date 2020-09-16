using System.Threading.Tasks;
using MailjetApiClient.Models;

namespace MailjetApiClient
{
    public interface IMailjetApiClient
    {
        Task<bool> SendMail(MailjetMail mailjetMail);
    }
}