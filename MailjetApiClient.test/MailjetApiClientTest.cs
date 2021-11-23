using Xunit;
using MailjetApiClient;
using System.Threading.Tasks;

namespace MailjetApiClient.test;

public class UnitTest1
{
    [Fact]
    async public Task SendMail()
    {
        var options = new MailjetOptions
        {
            ApiKeyPublic = "",
            ApiKeyPrivate = "",
            TestingRedirectionMail = "turbo.test@yopmail.com",
            SenderEmail = "",
            SenderName = "",
            IsSendingMailAllowed = true,
        };


        var client =  new MailjetService(options);
        await client.SendMail("t.dolley@unitee.io", 0);
    }
}